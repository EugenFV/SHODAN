//using Battle;
using UnityEngine;

public interface ILookController
{
    float ZoomFactor { get; set; }
    float X { get; set; }
    float Y { get; set; }
    float YMinLimit { get; set; }
    float YMaxLimit { get; set; }
    bool CurveShakeTestOffValue { get; set; }

    AudioSource AudioSource { get; }
    Vector3 FPSRotation { get; }
    //InputManager InputManager { get; }
    bool IsBlocked { get; }
    Camera MainCamera { get; }
    //SoldierCameraExtension SoldierCameraExtension { get; }
    Camera WeaponCamera { get; }

    void Block(bool block);
    Texture2D CaptureView(Vector2Int screenViewSize);
    void CustomLateUpdate();
    void CustomUpdate(bool isControllBlocked);
    void ExplosionShake(float power);
    //void Init(IFirstLookBattlePlayer localPlayer, ISoldierController soldierCtrl, PlayerVisibilityManager playerVisibilityMngr, IBattlePlayersList playersList, IBattleUnitsList battleUnitsList, ShotAssistSetting shotAssistSettings, IPlayersRelationshipsController playersRelationshipsCtrl);
    bool IsZoomingIn();
    void Kill();
    void Land(bool inAir);
    //void LaunchGrenade(BaseGrenadeViewController viewCtrl);
    void SetAimInput(bool aim);
    //void SetWeapon(BattleWeapon weapon);
    //void Shot(BaseWeaponViewController viewCtrl, WeaponType wt);
    void Spawn(Vector3 rotation);
}