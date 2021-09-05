//using Battle;
//using Battle.BattleUnits;
//using Battle.Killstreaks;
//using System.Collections.Generic;
//using UnityEngine;

//public class LookController : MonoBehaviour, ILookController
//{
//    [SerializeField]
//    private float yMinLimit = -90f;
//    [SerializeField]
//    private float yMaxLimit = 90f;

//    [SerializeField]
//    private float x = 0.0f;
//    [SerializeField]
//    private float y = 0.0f;

//    [SerializeField]
//    private float zoomFactor = 1F;

//    private bool _autoaimEnabled = true;

//    public float ZoomFactor
//    {
//        get { return zoomFactor; }
//        set { zoomFactor = value; }
//    }

//    public float X
//    {
//        get { return x; }
//        set { x = value; }
//    }
//    public float Y
//    {
//        get { return y; }
//        set { y = value; }
//    }

//    public float YMinLimit
//    {
//        get { return yMinLimit; }
//        set { yMinLimit = value; }
//    }
//    public float YMaxLimit
//    {
//        get { return yMaxLimit; }
//        set { yMaxLimit = value; }
//    }

//    [SerializeField]
//    private Camera _mainCamera = null, _weaponCamera = null;
//    [SerializeField]
//    private CameraExternalShakeController _camExternalShakeCtrl;
//    [SerializeField]
//    private MonoRecoilController _camShakeCtrl;
//    [SerializeField]
//    private CameraZoomController _camZoomCtrl;
//    [SerializeField]
//    private CameraWalkController _camWalkCtrl;
//    [SerializeField]
//    private WeaponShakeController _weaponShakeCtrl;
//    [SerializeField]
//    private AudioSource _audioSource = null;
//    [SerializeField]
//    private Transform _weaponContainer;

//    private WeaponRecoilSettings _recoilSettings => Penta.Settings.Instance.WeaponRecoil;
//    public Camera MainCamera { get { return _mainCamera; } }
//    public Camera WeaponCamera { get { return _weaponCamera; } }
//    public AudioSource AudioSource { get { return _audioSource; } }

//    [SerializeField]
//    private bool CurveShakeTestOff;
//    public bool CurveShakeTestOffValue
//    {
//        get { return CurveShakeTestOff; }
//        set { CurveShakeTestOff = value; }
//    }

//    public Vector3 FPSRotation { get; private set; } = Vector3.zero;

//    public SoldierCameraExtension SoldierCameraExtension { get; private set; }
//    public InputManager InputManager { get { return _localPlayer.SoldierController.InputManager; } }

//    public bool IsBlocked { get; private set; }

//    private AutoAimConfiguration<BaseThirdLookCombatUnit> _autoAimConfiguration;
//    private IFirstLookBattlePlayer _localPlayer;
//    private ISoldierController _soldierCtrl;
//    private IBattlePlayersList _playersCtrl;
//    private IBattleUnitsList _battleUnitsList;
//    private PlayerVisibilityManager _playerVisibilityMngr;
//    private IPlayersRelationshipsController _playersRelationshipsCtrl;

//    private Vector2 _touchStartPosition;
//    private Vector2 _touchLastPosition;
//    private int _axisSmoothAmount = 4;
//    private List<Vector2> _axisPositions;
//    private Vector2 _axisPosition;
//    private int _touchID = -1;

//    public void Init(IFirstLookBattlePlayer localPlayer, ISoldierController soldierCtrl, PlayerVisibilityManager playerVisibilityMngr,
//        IBattlePlayersList playersList, IBattleUnitsList battleUnitsList, ShotAssistSetting shotAssistSettings, IPlayersRelationshipsController playersRelationshipsCtrl)
//    {
//        _localPlayer = localPlayer;
//        _soldierCtrl = soldierCtrl;
//        _playersCtrl = playersList;
//        _battleUnitsList = battleUnitsList;
//        _playerVisibilityMngr = playerVisibilityMngr;
//        _playersRelationshipsCtrl = playersRelationshipsCtrl;

//        MainCamera.fieldOfView = BaseSoldierController.NORMAL_ZOOM_MODE;// * zoomFactor;
//        WeaponCamera.gameObject.SetActive(true);

//        InitConfiguration();
//        SetupShotAssist(shotAssistSettings);

//        _camZoomCtrl.Init(_soldierCtrl, WeaponCamera, MainCamera, this);
//        _camShakeCtrl.Init(_soldierCtrl, _localPlayer.ShotController);
//        _camWalkCtrl.Init(_soldierCtrl, WeaponCamera, MainCamera, this);
//        _weaponShakeCtrl.Init(_soldierCtrl);

//        Block(true);

//        _soldierCtrl.LocalPlayer.OnEffectStart -= OnEffectStart;
//        _soldierCtrl.LocalPlayer.OnEffectEnd -= OnEffectEnd;

//        _soldierCtrl.LocalPlayer.OnEffectStart += OnEffectStart;
//        _soldierCtrl.LocalPlayer.OnEffectEnd += OnEffectEnd;
//    }

//    private void InitConfiguration()
//    {
//        if (_autoAimConfiguration == null)
//        {
//            _autoAimConfiguration = new AutoAimConfiguration<BaseThirdLookCombatUnit>(
//                10, 45, 0.5f, 1, 14, 25, 30,
//                cp => cp != null && cp.MustTakePartInAutoAiming && _playersRelationshipsCtrl.IsEnemyForLocalPlayer(cp),
//                null);
//        }

//        SoldierCameraExtension = gameObject.AddComponent<SoldierCameraExtension>();
//        Vector3 angles = transform.eulerAngles;
//        X = angles.y;
//        Y = angles.x;
//    }

//    private void SetupShotAssist(ShotAssistSetting setting)
//    {
//        _autoAimConfiguration = new AutoAimConfiguration<BaseThirdLookCombatUnit>(
//            setting.MinAngle,
//            setting.MaxAngle,
//            setting.LowAngle,
//            setting.MinPower,
//            setting.MaxPower,
//            setting.ZoomPower,
//            setting.MinDistance,
//            cp => cp != null && cp.MustTakePartInAutoAiming && _playersRelationshipsCtrl.IsEnemyForLocalPlayer(cp),
//            null);
//    }

//    public void Block(bool block)
//    {
//        IsBlocked = block;
//        if (enabled != !block)
//        {
//            enabled = !block;
//        }
//    }

//    public Texture2D CaptureView(Vector2Int screenViewSize)
//    {
//        return Penta.Utils.ScreenshotByCamera(screenViewSize.x / 3, screenViewSize.y / 3, _mainCamera); // 3 means reduce quality of screenshot
//    }

//    private void OnDisable()
//    {
//        if (_soldierCtrl != null && _soldierCtrl.LocalPlayer != null)
//        {
//            _soldierCtrl.LocalPlayer.OnEffectStart -= OnEffectStart;
//            _soldierCtrl.LocalPlayer.OnEffectEnd -= OnEffectEnd;
//        }
//    }

//    public void Spawn(Vector3 rotation)
//    {
//        ResetRotation(rotation.y, rotation.x);
//        Block(false);
//        gameObject.SetActive(true);
//        MainCamera.gameObject.SetActive(true);
//        _camShakeCtrl.Resurrect();
//        _camExternalShakeCtrl.Resurrect();
//        _camZoomCtrl.Resurrect();
//    }

//    public void SetAimInput(bool aim)
//    {
//        _camZoomCtrl.SetAimInput(aim);
//        _camWalkCtrl.SetAimInput(aim);
//    }

//    public void SetWeapon(BattleWeapon weapon)
//    {
//        _camWalkCtrl.SetWeapon(weapon);
//    }

//    public bool IsZoomingIn()
//    {
//        return _camZoomCtrl.IsZoomingIn();
//    }

//    public void Kill()
//    {
//        Block(true);
//        MainCamera.gameObject.SetActive(false);
//        _camZoomCtrl.Kill();
//    }

//    private void ResetRotation(float horizontal, float vertical)
//    {
//        Y = vertical;
//        X = horizontal;
//    }

//    public void CustomUpdate(bool isControllBlocked)
//    {
//        if (isControllBlocked || IsBlocked)
//        {
//            return;
//        }

//        GetInputMobileSmoothAccurate();
//        RotateSoldier();
//    }

//    public void CustomLateUpdate()
//    {
//        if (IsBlocked)
//        {
//            return;
//        }

//        _camZoomCtrl.UpdateController();
//        _camWalkCtrl.UpdateController();
//        _camShakeCtrl.UpdateController();
//        _camExternalShakeCtrl.UpdateController();
//        _weaponShakeCtrl.UpdateController();
//        _mainCamera.transform.localRotation = Quaternion.Euler(FPSRotation) * Quaternion.Euler(_camShakeCtrl.ShakeRotation) * Quaternion.Euler(_camExternalShakeCtrl.ShakeRotation) * _camWalkCtrl.CameraWalkRotation;
//        _weaponCamera.fieldOfView = _camZoomCtrl.WeaponCameraFOVValue;
//        _mainCamera.fieldOfView = _camZoomCtrl.MainCameraFOVValue;
//        _weaponContainer.transform.localPosition = _weaponShakeCtrl.ShakePosition + _camWalkCtrl.CurrentPosition + _camZoomCtrl.WeaponCorrectionPosition;
//        _weaponContainer.transform.localRotation = _camWalkCtrl.CurrentRotation;
//    }

//    private void GetInputMobileSmoothAccurate()
//    {
//        Vector2 axisControlAdd = Vector2.zero;
//        if (_axisPositions == null)
//        {
//            _axisPositions = new List<Vector2>();
//        }
//        Vector2 axisAdd = Vector3.zero;

//        GetAxisCorrection(ref axisAdd);
//#if (UNITY_EDITOR || UNITY_STANDALONE) && !FORCE_USE_MOBILE_CONTROL
//        GetInputEditor(ref axisControlAdd);
//#else
//        GetInputTouch(ref axisControlAdd);
//#endif
//        if (_axisPositions.Count == 0)
//        {
//            return;
//        }
//        float xAdd = 0;
//        float yAdd = 0;
//        foreach (Vector2 axis in _axisPositions)
//        {
//            xAdd += axis.x;
//            yAdd += axis.y;
//        }
//        axisControlAdd = new Vector2((xAdd / _axisPositions.Count), (yAdd / _axisPositions.Count));
//        if (OptionsManager.EnableAutoaim)
//        {
//            X += Mix(axisControlAdd.x, axisAdd.x);
//            Y += Mix(axisControlAdd.y, axisAdd.y);
//        }
//        else
//        {
//            X += axisControlAdd.x;
//            Y += axisControlAdd.y;
//        }
//        Y = ClampAngle(Y, YMinLimit, YMaxLimit);
//    }

//    private void GetAxisCorrection(ref Vector2 axisAdd)
//    {
//        if (_autoaimEnabled)
//        {
//            IShotController _shotController = _soldierCtrl.LocalPlayer.ShotController;
//            IWeaponController _weaponController = _soldierCtrl.LocalPlayer.WeaponController;
//            if (!DontNeedAim())
//            {
//                if (_weaponController.CurrentWeapon != null)
//                {
//                    WeaponType weaponType = _weaponController.CurrentWeapon.Type;
//                    _autoAimConfiguration.SetZoom(_shotController.Zoom && weaponType == WeaponType.SNIPER_RIFLE);
//                    ShotAssistant<BaseThirdLookCombatUnit>.FindAutoCorrectionPartViewportProject(
//                        ref axisAdd, _autoAimConfiguration, ShotAssistant<BaseThirdLookCombatUnit>.GetTargets(_playersCtrl, _battleUnitsList, _playersRelationshipsCtrl), MainCamera, weaponType, _playerVisibilityMngr);
//                }
//            }
//        }
//    }

//    private bool DontNeedAim()
//    {
//        IBaseGrenadesController grenadesCtrl = _soldierCtrl.LocalPlayer.GrenadesCtrl;
//        if (grenadesCtrl.IsAnyGrenadeInAction)
//        {
//            return true;
//        }
//        return _soldierCtrl.LocalPlayer.BattleKillstreakCtrl.LastLaunchedKillstreak != null
//            && (_soldierCtrl.LocalPlayer.BattleKillstreakCtrl.LastLaunchedKillstreak.CurrentState == KillstreakState.Launching
//            || _soldierCtrl.LocalPlayer.BattleKillstreakCtrl.LastLaunchedKillstreak.CurrentState == KillstreakState.Activating);
//    }

//    private void OnEffectEnd(BaseCombatUnit view, ImpactType impactType)
//    {
//        if (impactType == ImpactType.Blindness)
//        {
//            _autoaimEnabled = true;
//        }
//    }

//    private void OnEffectStart(BaseCombatUnit view, ImpactType impactType, byte arg3)
//    {
//        if (impactType == ImpactType.Blindness)
//        {
//            _autoaimEnabled = false;
//        }
//    }

//    private void GetInputEditor(ref Vector2 axisControlAdd)
//    {
//        while (_axisPositions.Count >= _axisSmoothAmount)
//        {
//            _axisPositions.RemoveAt(0);
//        }
//        axisControlAdd = new Vector2(Input.GetAxis("Mouse X") * Time.deltaTime * OptionsManager.MouseSens * ZoomFactor * 10, -Input.GetAxis("Mouse Y") * Time.deltaTime * OptionsManager.MouseSens * ZoomFactor * 10 / 2);

//        _axisPosition = new Vector2(axisControlAdd.x, axisControlAdd.y);
//        _axisPositions.Add(new Vector2(_axisPosition.x, _axisPosition.y));
//    }

//    private void GetInputTouch(ref Vector2 axisControlAdd)
//    {
//        InputManager.UpdateTouches();
//        while (_axisPositions.Count >= _axisSmoothAmount)
//        {
//            _axisPositions.RemoveAt(0);
//        }

//        if (!InputManager.Touches.ContainsKey(_touchID))
//        {
//            _touchID = -1;
//        }

//        foreach (TouchInstance touch in InputManager.Touches.Values)
//        {
//            if (touch.Touch.phase == TouchPhase.Moved && touch.StartPosition.x > Screen.width * 0.5f)
//            {
//                if (_touchID == -1)
//                {
//                    _touchID = touch.Touch.fingerId;
//                }

//                if (touch.Touch.fingerId != _touchID)
//                {
//                    break;
//                }

//                Vector2 delta = Vector2.zero;
//                if (_touchStartPosition != touch.StartPosition)
//                {
//                    _touchLastPosition = _touchStartPosition = touch.StartPosition;
//                }
//                else
//                {
//                    delta = touch.Touch.position - _touchLastPosition;
//                    _touchLastPosition = touch.Touch.position;
//                }
//                float deltaT = 0.006f;
//                float sensity = OptionsManager.MouseSens * ZoomFactor;
//                float coefficient = sensity * deltaT;
//                axisControlAdd = new Vector2(delta.x, -delta.y * 0.5f) * coefficient;
//                break;
//            }
//        }
//        _axisPosition = new Vector2(axisControlAdd.x, axisControlAdd.y);
//        _axisPositions.Add(new Vector2(_axisPosition.x, _axisPosition.y));
//    }

//    private float Mix(float x, float x1)
//    {
//        float res;
//        if (Mathf.Sign(x) == Mathf.Sign(x1))
//        {
//            if (x < 0 || x1 < 0)
//            {
//                res = Mathf.Min(x, x1);
//            }
//            else
//            {
//                res = Mathf.Max(x, x1);
//            }
//        }
//        else
//        {
//            res = x + x1;
//        }
//        return res;
//    }

//    private static float ClampAngle(float angle, float min, float max)
//    {
//        if (angle < -360f)
//        {
//            angle += 360f;
//        }

//        if (angle > 360f)
//        {
//            angle -= 360f;
//        }

//        return Mathf.Clamp(angle, min, max);
//    }

//    private void RotateSoldier()
//    {
//        if (SoldierCameraExtension.BlockRotation)
//        {
//            return;
//        }
//        _soldierCtrl.SetOrientation(new Vector3(X, Y, 0));
//        FPSRotation = new Vector3(Y, 0f, 0f);
//    }

//    public void Shot(BaseWeaponViewController viewCtrl, WeaponType wt)
//    {
//        if (viewCtrl == null)
//        {
//            Debug.LogErrorFormat("[LookController.Shot] viewCtrl is null!");
//            return;
//        }

//        _camShakeCtrl.Shake(wt);
//        if (_localPlayer.ShotController.Zoom && (viewCtrl.HasAttachedAim() || !CurveShakeTestOff))
//        {
//            _weaponShakeCtrl.Shake();
//        }
//        viewCtrl.Shot();
//    }

//    public void LaunchGrenade(BaseGrenadeViewController viewCtrl)
//    {
//        viewCtrl.LaunchGrenade();
//    }

//    public void ExplosionShake(float power)
//    {
//        _camExternalShakeCtrl.ExplosionShake(power);
//    }

//    public void Land(bool inAir)
//    {
//        _camWalkCtrl.Land(inAir);
//    }
//}
