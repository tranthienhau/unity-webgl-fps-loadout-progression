using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Breachpoint.Data;
using Breachpoint.UI;

namespace Breachpoint.App
{
    // Playable single-player arena slice. Built entirely in code (primitives + Standard shader).
    // Hitscan shooting is driven by the equipped weapon's stats; AI bots stand in for opponents
    // (true multiplayer needs server netcode, handled backend-side per the brief).
    public class GameplayController : MonoBehaviour
    {
        AppController _app;
        GameMode _mode;
        Weapon _weapon;

        // player
        CharacterController _cc;
        Camera _cam;
        float _pitch, _yaw;
        Vector3 _velocity;
        const float MoveSpeed = 6f, Gravity = -18f, JumpVel = 6.5f, EyeHeight = 1.7f;
        const float MouseSens = 2.2f;
        int _hp = 100, _maxHp = 100;
        float _regenTimer;

        // weapon (derived from stats)
        int _rpm, _magSize, _ammo, _reserve, _dmg;
        float _range, _spreadDeg, _shotCooldown, _nextShot, _reloadTimer;
        bool _reloading;

        // bots / score
        readonly List<Bot> _bots = new();
        int _score;
        const int BotCount = 6;
        float _arenaHalf = 24f;

        // hud
        Text _hpText, _ammoText, _scoreText, _hitMarker, _msg;
        Image _hpFill;
        LineRenderer _tracer; float _tracerTimer;
        Light _muzzle; float _muzzleTimer;

        public void Begin(AppController app, GameMode mode, Weapon weapon)
        {
            _app = app; _mode = mode; _weapon = weapon;
            DeriveWeapon(weapon);
            BuildLighting();
            BuildArena();
            BuildPlayer();
            for (int i = 0; i < BotCount; i++) SpawnBot();
            BuildHud();
            LockCursor(true);
        }

        void DeriveWeapon(Weapon w)
        {
            var s = w.Stats;
            _rpm = Mathf.RoundToInt(Mathf.Lerp(120, 900, s.FireRate / 100f));
            _shotCooldown = 60f / _rpm;
            _dmg = Mathf.RoundToInt(Mathf.Lerp(18, 60, s.Damage / 100f));
            _range = Mathf.Lerp(40, 200, s.Range / 100f);
            _spreadDeg = Mathf.Lerp(3.5f, 0.3f, s.Accuracy / 100f);
            _magSize = w.Class switch
            {
                WeaponClass.Sniper or WeaponClass.Marksman => 6,
                WeaponClass.Shotgun => 8,
                WeaponClass.Pistol => 17,
                WeaponClass.SMG => 40,
                WeaponClass.LMG => 100,
                _ => 30
            };
            _ammo = _magSize; _reserve = _magSize * 5;
        }

        // ---------------- world ----------------
        static Material Mat(Color c, float metallic = 0.1f, float smooth = 0.3f)
        {
            var m = new Material(Shader.Find("Standard"));
            m.color = c;
            m.SetFloat("_Metallic", metallic);
            m.SetFloat("_Glossiness", smooth);
            return m;
        }

        void BuildLighting()
        {
            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(0.8f, 0.9f, 1f);
            sun.intensity = 1.1f;
            sun.transform.rotation = Quaternion.Euler(52, 40, 0);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.18f, 0.20f, 0.26f);
        }

        void BuildArena()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.localScale = new Vector3(_arenaHalf / 5f * 2, 1, _arenaHalf / 5f * 2);
            floor.GetComponent<Renderer>().material = Mat(Theme.Hex("12161D"), 0.2f, 0.25f);
            floor.transform.SetParent(transform, false);

            // perimeter walls
            float h = 4f;
            BuildWall(new Vector3(0, h / 2, _arenaHalf), new Vector3(_arenaHalf * 2, h, 1));
            BuildWall(new Vector3(0, h / 2, -_arenaHalf), new Vector3(_arenaHalf * 2, h, 1));
            BuildWall(new Vector3(_arenaHalf, h / 2, 0), new Vector3(1, h, _arenaHalf * 2));
            BuildWall(new Vector3(-_arenaHalf, h / 2, 0), new Vector3(1, h, _arenaHalf * 2));

            // scattered cover crates (deterministic layout)
            var rng = new System.Random(42);
            var crateMat = Mat(Theme.Hex("1C222C"), 0.3f, 0.4f);
            var accentMat = Mat(Theme.Primary * 0.6f, 0.5f, 0.6f);
            for (int i = 0; i < 14; i++)
            {
                var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                float sx = 1.2f + (float)rng.NextDouble() * 2.5f;
                float sy = 1f + (float)rng.NextDouble() * 2.5f;
                box.transform.localScale = new Vector3(sx, sy, sx);
                float x = (float)(rng.NextDouble() * 2 - 1) * (_arenaHalf - 4);
                float z = (float)(rng.NextDouble() * 2 - 1) * (_arenaHalf - 4);
                box.transform.position = new Vector3(x, sy / 2, z);
                box.GetComponent<Renderer>().material = (i % 4 == 0) ? accentMat : crateMat;
                box.transform.SetParent(transform, false);
            }
        }

        void BuildWall(Vector3 pos, Vector3 scale)
        {
            var w = GameObject.CreatePrimitive(PrimitiveType.Cube);
            w.transform.position = pos; w.transform.localScale = scale;
            w.GetComponent<Renderer>().material = Mat(Theme.Hex("0C0F14"), 0.1f, 0.2f);
            w.transform.SetParent(transform, false);
        }

        void BuildPlayer()
        {
            var p = new GameObject("Player");
            p.transform.SetParent(transform, false);
            p.transform.position = new Vector3(0, 1.1f, -_arenaHalf + 4);
            _cc = p.AddComponent<CharacterController>();
            _cc.height = 1.8f; _cc.radius = 0.4f; _cc.center = new Vector3(0, 0.9f, 0);

            var camGo = new GameObject("PlayerCamera");
            camGo.transform.SetParent(p.transform, false);
            camGo.transform.localPosition = new Vector3(0, EyeHeight, 0);
            _cam = camGo.AddComponent<Camera>();
            _cam.tag = "MainCamera";
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = Theme.Hex("0A0D12");
            _cam.fieldOfView = 70;
            _cam.nearClipPlane = 0.05f;

            _muzzle = new GameObject("Muzzle").AddComponent<Light>();
            _muzzle.transform.SetParent(camGo.transform, false);
            _muzzle.transform.localPosition = new Vector3(0.2f, -0.1f, 0.6f);
            _muzzle.type = LightType.Point; _muzzle.range = 8; _muzzle.color = Theme.Primary; _muzzle.intensity = 0; _muzzle.enabled = true;

            var tg = new GameObject("Tracer");
            tg.transform.SetParent(transform, false);
            _tracer = tg.AddComponent<LineRenderer>();
            _tracer.material = Mat(Theme.Primary); _tracer.startWidth = 0.03f; _tracer.endWidth = 0.01f;
            _tracer.positionCount = 2; _tracer.enabled = false;
        }

        class Bot
        {
            public GameObject go;
            public Renderer rend;
            public int hp;
            public float respawn;
        }

        void SpawnBot(Bot reuse = null)
        {
            var bot = reuse ?? new Bot();
            if (bot.go == null)
            {
                bot.go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                bot.go.name = "Bot";
                bot.go.transform.SetParent(transform, false);
                bot.rend = bot.go.GetComponent<Renderer>();
                Object.Destroy(bot.go.GetComponent<Collider>());
                bot.go.AddComponent<CapsuleCollider>();
                _bots.Add(bot);
            }
            bot.hp = 100;
            bot.rend.material = Mat(Theme.Secondary, 0.2f, 0.4f);
            var rng = new System.Random(System.Environment.TickCount + _bots.Count);
            float x = (float)(rng.NextDouble() * 2 - 1) * (_arenaHalf - 3);
            float z = (float)(rng.NextDouble() * 2 - 1) * (_arenaHalf - 3);
            bot.go.transform.position = new Vector3(x, 1f, z);
            bot.go.SetActive(true);
            bot.respawn = 0;
        }

        // ---------------- loop ----------------
        void Update()
        {
            if (_cc == null) return;
            if (Input.GetKeyDown(KeyCode.Escape)) { _app.EndMatch(); return; }
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                if (Input.GetMouseButtonDown(0)) LockCursor(true);
                return;
            }

            Look();
            Move();
            Shoot();
            Reload();
            UpdateBots();
            UpdateFx();
            UpdateHud();
        }

        void Look()
        {
            _yaw += Input.GetAxis("Mouse X") * MouseSens;
            _pitch -= Input.GetAxis("Mouse Y") * MouseSens;
            _pitch = Mathf.Clamp(_pitch, -85, 85);
            _cc.transform.rotation = Quaternion.Euler(0, _yaw, 0);
            _cam.transform.localRotation = Quaternion.Euler(_pitch, 0, 0);
        }

        void Move()
        {
            float x = Input.GetAxisRaw("Horizontal"), z = Input.GetAxisRaw("Vertical");
            var dir = (_cc.transform.right * x + _cc.transform.forward * z).normalized;
            float speed = MoveSpeed * (Input.GetKey(KeyCode.LeftShift) ? 1.4f : 1f);
            if (_cc.isGrounded)
            {
                _velocity.y = -1f;
                if (Input.GetKeyDown(KeyCode.Space)) _velocity.y = JumpVel;
            }
            _velocity.y += Gravity * Time.deltaTime;
            _cc.Move((dir * speed + Vector3.up * _velocity.y) * Time.deltaTime);

            // health regen
            _regenTimer += Time.deltaTime;
            if (_regenTimer > 2f && _hp < _maxHp) { _hp = Mathf.Min(_maxHp, _hp + 1); }
        }

        void Shoot()
        {
            if (_reloading) return;
            bool fire = Input.GetMouseButton(0);
            if (fire && Time.time >= _nextShot && _ammo > 0)
            {
                _nextShot = Time.time + _shotCooldown;
                _ammo--;
                FireRay();
                _pitch -= Random.Range(0.4f, 1.1f); // recoil kick
                _muzzleTimer = 0.05f; _muzzle.intensity = 3.5f;
            }
            if (_ammo == 0 && _reserve > 0 && fire) StartReload();
        }

        void FireRay()
        {
            var rot = Quaternion.Euler(Random.Range(-_spreadDeg, _spreadDeg), Random.Range(-_spreadDeg, _spreadDeg), 0);
            var dir = rot * _cam.transform.forward;
            var origin = _cam.transform.position;
            bool hitBot = false;
            Vector3 end = origin + dir * _range;
            if (Physics.Raycast(origin, dir, out var hit, _range))
            {
                end = hit.point;
                foreach (var b in _bots)
                {
                    if (b.go.activeSelf && hit.collider == b.go.GetComponent<Collider>())
                    {
                        b.hp -= _dmg; hitBot = true;
                        if (b.hp <= 0) { KillBot(b); }
                        break;
                    }
                }
            }
            // tracer
            _tracer.SetPosition(0, _muzzle.transform.position);
            _tracer.SetPosition(1, end);
            _tracer.enabled = true; _tracerTimer = 0.04f;
            if (hitBot) FlashHitMarker();
        }

        void KillBot(Bot b)
        {
            _score++;
            b.go.SetActive(false);
            b.respawn = Time.time + 1.5f;
        }

        void StartReload() { _reloading = true; _reloadTimer = ReloadTime(); }
        float ReloadTime() => _weapon.Class is WeaponClass.LMG ? 4f : _weapon.Class is WeaponClass.Sniper ? 2.6f : 1.8f;

        void Reload()
        {
            if (Input.GetKeyDown(KeyCode.R) && !_reloading && _ammo < _magSize && _reserve > 0) StartReload();
            if (!_reloading) return;
            _reloadTimer -= Time.deltaTime;
            if (_reloadTimer <= 0)
            {
                int need = _magSize - _ammo;
                int take = Mathf.Min(need, _reserve);
                _ammo += take; _reserve -= take; _reloading = false;
            }
        }

        void UpdateBots()
        {
            var ppos = _cc.transform.position;
            foreach (var b in _bots)
            {
                if (!b.go.activeSelf)
                {
                    if (Time.time >= b.respawn) SpawnBot(b);
                    continue;
                }
                // seek player, keep on floor
                var to = ppos - b.go.transform.position; to.y = 0;
                float dist = to.magnitude;
                if (dist > 2.2f)
                    b.go.transform.position += to.normalized * 2.4f * Time.deltaTime;
                else if (dist < 2.2f)
                {
                    // melee tick
                    _hp = Mathf.Max(0, _hp - Mathf.CeilToInt(8 * Time.deltaTime));
                    _regenTimer = 0;
                    if (_hp <= 0) { _hp = _maxHp; _cc.transform.position = new Vector3(0, 1.1f, -_arenaHalf + 4); }
                }
                b.go.transform.position = new Vector3(b.go.transform.position.x, 1f, b.go.transform.position.z);
            }
        }

        void UpdateFx()
        {
            if (_muzzleTimer > 0) { _muzzleTimer -= Time.deltaTime; if (_muzzleTimer <= 0) _muzzle.intensity = 0; }
            if (_tracerTimer > 0) { _tracerTimer -= Time.deltaTime; if (_tracerTimer <= 0) _tracer.enabled = false; }
            if (_hitMarker != null && _hitMarker.color.a > 0)
            {
                var c = _hitMarker.color; c.a = Mathf.Max(0, c.a - Time.deltaTime * 4); _hitMarker.color = c;
            }
        }

        void FlashHitMarker() { if (_hitMarker != null) _hitMarker.color = new Color(1, 1, 1, 1); }

        // ---------------- hud ----------------
        void BuildHud()
        {
            var go = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            var canvas = go.GetComponent<Canvas>();
            // ScreenSpace-Camera (through the player camera) so the HUD renders in WebGL and is
            // also captured by the headless screenshot harness.
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = _cam;
            canvas.planeDistance = 0.5f;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            var root = (RectTransform)go.transform;

            // crosshair
            var ch = UIFactory.Label(root, "+", Theme.Body, 40, Theme.Primary, TextAnchor.MiddleCenter);
            Center(ch.rectTransform, 60, 60);
            _hitMarker = UIFactory.Label(root, "✕", Theme.Body, 30, new Color(1, 1, 1, 0), TextAnchor.MiddleCenter);
            Center(_hitMarker.rectTransform, 60, 60);

            // score (top center)
            var scoreBox = UIFactory.Card(root, new Color(0, 0, 0, 0.5f), Theme.Primary, 1);
            Anchor(scoreBox.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -16), 220, 56);
            UIFactory.AddHLayout(scoreBox.rectTransform, 10, new RectOffset(16, 16, 8, 8), false, true, true);
            UIFactory.Label(scoreBox.transform, _mode.Name, Theme.Body, 14, Theme.TextDim, TextAnchor.MiddleLeft);
            _scoreText = UIFactory.Label(scoreBox.transform, "0", Theme.Display, 26, Theme.Primary, TextAnchor.MiddleRight, FontStyle.Bold);

            // health (bottom-left)
            var hpBox = UIFactory.NewRect("HP", root);
            Anchor(hpBox, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(40, 40), 320, 50);
            UIFactory.AddVLayout(hpBox, 4, default, true, false, true);
            var hr = UIFactory.NewRect("hr", hpBox); UIFactory.AddHLayout(hr, 8, default, false, false, true);
            UIFactory.Label(hr.transform, "HEALTH", Theme.Mono, 11, Theme.TextDim, TextAnchor.MiddleLeft);
            _hpText = UIFactory.Label(hr.transform, "100", Theme.Mono, 12, Theme.Primary, TextAnchor.MiddleLeft);
            var track = UIFactory.Panel(hpBox, new Color(1, 1, 1, 0.08f), "t"); UIFactory.SetHeight(track.gameObject, 10);
            _hpFill = UIFactory.Panel(track.transform, Theme.Primary, "f");
            _hpFill.rectTransform.anchorMin = Vector2.zero; _hpFill.rectTransform.anchorMax = Vector2.one;
            _hpFill.rectTransform.offsetMin = Vector2.zero; _hpFill.rectTransform.offsetMax = Vector2.zero;

            // ammo (bottom-right)
            var ammoBox = UIFactory.NewRect("Ammo", root);
            Anchor(ammoBox, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-40, 40), 240, 56);
            UIFactory.AddVLayout(ammoBox, 0, default, true, false, true);
            UIFactory.Label(ammoBox.transform, _weapon.Name, Theme.Body, 14, Theme.TextDim, TextAnchor.MiddleRight);
            _ammoText = UIFactory.Label(ammoBox.transform, "", Theme.Display, 30, Theme.Text, TextAnchor.MiddleRight, FontStyle.Bold);

            // controls hint (top-left)
            _msg = UIFactory.Label(root, "WASD move  -  MOUSE aim  -  CLICK fire  -  R reload  -  SHIFT sprint  -  ESC exit",
                Theme.Mono, 12, Theme.TextDim, TextAnchor.UpperLeft);
            Anchor(_msg.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -32), 900, 24);
        }

        void UpdateHud()
        {
            _scoreText.text = _score.ToString();
            _hpText.text = _hp.ToString();
            _hpFill.rectTransform.anchorMax = new Vector2(_hp / (float)_maxHp, 1);
            _hpFill.color = _hp > 40 ? Theme.Primary : Theme.Secondary;
            _ammoText.text = _reloading ? "RELOADING" : $"{_ammo} / {_reserve}";
            _ammoText.color = _ammo == 0 ? Theme.Secondary : Theme.Text;
        }

        static void Center(RectTransform rt, float w, float h)
            => Anchor(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, w, h);

        static void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot, Vector2 pos, float w, float h)
        {
            rt.anchorMin = min; rt.anchorMax = max; rt.pivot = pivot;
            rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(w, h);
        }

        void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        void OnDestroy() { LockCursor(false); }
    }
}
