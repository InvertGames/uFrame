
using System;
using System.Collections;
using System.Linq;
using UnityEngine;


public partial class FPSCrosshair {
    public Texture2D _CrossHairTexture;
    public float _CrossHairScale;
    private FPSWeaponFire _weaponFire;

    public FPSWeaponFire FPSWeaponFire
    {
        get { return _weaponFire ?? (_weaponFire = this.GetComponent<FPSWeaponFire>()); }
    }

    public void ResetCrosshair()
    {
        _CrossHairScale = FPSWeapon.ZoomIndex == 1 ? 0.02f : 0.15f;
    }

    public void OnGUI()
    {

        if (FPSWeapon.State == FPSWeaponState.Reloading || FPSWeapon.State == FPSWeaponState.Empty) return;

        var textureWidth = Mathf.Max(
            (_CrossHairTexture.width * _CrossHairScale) * FPSWeaponFire._Spread * 200f,
            _CrossHairTexture.width * _CrossHairScale);
        var textureHeight = Mathf.Max(
            (_CrossHairTexture.height * _CrossHairScale) * FPSWeaponFire._Spread * 200f,
            _CrossHairTexture.height * _CrossHairScale);

        textureWidth = Math.Min(textureWidth, _CrossHairTexture.width);
        textureHeight = Math.Min(textureHeight, _CrossHairTexture.height);

        var textureXCenter = textureWidth / 2f;
        var textureYCenter = textureHeight / 2f;

        var screenX = (Screen.width / 2f) - textureXCenter;
        var screenY = (Screen.height / 2f) - textureYCenter;

        GUI.color = Color.yellow;
        GUI.DrawTexture(new Rect(screenX, screenY,
            textureWidth,
            textureHeight), _CrossHairTexture, ScaleMode.StretchToFill, true);
    }
}
