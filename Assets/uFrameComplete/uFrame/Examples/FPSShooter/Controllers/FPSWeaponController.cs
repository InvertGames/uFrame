using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class FPSWeaponController : FPSWeaponControllerBase
{
    public override void NextZoom(FPSWeaponViewModel fpsWeapon)
    {
        if (fpsWeapon.MaxZooms - 1 == fpsWeapon.ZoomIndex)
        {
            fpsWeapon.ZoomIndex = 0;
        }
        else
        {
            fpsWeapon.ZoomIndex++;
        }
    }

    public override void InitializeFPSWeapon(FPSWeaponViewModel fPSWeapon)
    {
        fPSWeapon.State = FPSWeaponState.Active;
        
    }

    public override void BeginFire(FPSWeaponViewModel weapon)
    {
        weapon.State = FPSWeaponState.Firing;
        if (weapon.Ammo < 1)
        {
            weapon.State = FPSWeaponState.Empty;
        }
    }

    public override void EndFire(FPSWeaponViewModel weapon)
    {
        weapon.State = FPSWeaponState.Active;
    }

    public override void BulletFired(FPSWeaponViewModel fPSWeapon)
    {
        base.BulletFired(fPSWeapon);
        fPSWeapon.Ammo -= 1;
        if (fPSWeapon.Ammo < 1)
        {
            fPSWeapon.State = FPSWeaponState.Empty;
        }
    }

    public override IEnumerator Reload(FPSWeaponViewModel fpsWeapon)
    {
        fpsWeapon.State = FPSWeaponState.Reloading;
        yield return new WaitForSeconds(fpsWeapon.ReloadTime);
        fpsWeapon.Ammo = fpsWeapon.RoundSize;
        fpsWeapon.State = FPSWeaponState.Active;
    }
}