//using Battle;
//using UnityEngine;

//public class LookControllerForSpectating : MonoBehaviour, ILookController
//{
//    public float ZoomFactor
//    {
//        get { return 0f; }
//        set { }
//    }
//    public float X
//    {
//        get { return 0f; }
//        set { }
//    }
//    public float Y
//    {
//        get { return 0f; }
//        set { }
//    }
//    public float YMinLimit
//    {
//        get { return 0f; }
//        set { }
//    }
//    public float YMaxLimit
//    {
//        get { return 0f; }
//        set { }
//    }
//    public bool CurveShakeTestOffValue
//    {
//        get { return false; }
//        set { }
//    }
//    public bool IsBlocked => true;
//    public Vector3 FPSRotation => Vector3.zero;

//    [SerializeField]
//    private Camera _mainCamera = null, _weaponCamera = null;
//    [SerializeField]
//    private AudioSource _audioSource = null;

//    private IFirstLookBattlePlayer _localPlayer;

//    public AudioSource AudioSource => _audioSource;
//    public InputManager InputManager => _localPlayer.SoldierController.InputManager;
//    public Camera MainCamera => _mainCamera;
//    public SoldierCameraExtension SoldierCameraExtension => null;
//    public Camera WeaponCamera => _weaponCamera;

//    public void Init(IFirstLookBattlePlayer localPlayer, ISoldierController soldierCtrl, PlayerVisibilityManager playerVisibilityMngr, IBattlePlayersList playersList, IBattleUnitsList battleUnitsList, ShotAssistSetting shotAssistSettings, IPlayersRelationshipsController playersRelationshipsCtrl)
//    {
//        _localPlayer = localPlayer;
//    }

//    public void Block(bool block) { }
//    public Texture2D CaptureView(Vector2Int screenViewSize) => null;
//    public void CustomLateUpdate() { }
//    public void CustomUpdate(bool isControllBlocked) { }
//    public void ExplosionShake(float power) { }
//    public bool IsZoomingIn() => false;
//    public void Kill() { }
//    public void Land(bool inAir) { }
//    public void LaunchGrenade(BaseGrenadeViewController viewCtrl) { }
//    public void SetAimInput(bool aim) { }
//    public void SetWeapon(BattleWeapon weapon) { }
//    public void Shot(BaseWeaponViewController viewCtrl, WeaponType wt) { }
//    public void Spawn(Vector3 rotation) { }
//}
