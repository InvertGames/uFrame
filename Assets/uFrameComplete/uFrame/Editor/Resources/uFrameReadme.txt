[u]Frame Release Notes
IMPORTANT!!!: REGENERATE ANY DIAGRAMS!
1.1
- Modified: Dependency injection RegisterInstance has some reordered parameters to illeminate confusion and odd overloads


1.06 5/10/2014
- Fixed: Heirarchy Icons going missing
- Fixed: When creating views that are not connected they won't be generated as an empty file.
- Fixed: You can't delete items that contain other items.  You must delete the "inner" items first.
- Fixed: When removing things it correctly removes the associations so that cascading errors don't occur. This includes Views still being generated
- Fixed: Add to scene on scene managers not working correctly
- Fixed: Some errors that occur from other non uFrame corrupted assets when polling for diagrams.
- Fixed: Other minor issues and fixes
- Modified: Now you can use ForceResolve directly from a view without manually adding the named injection.
- Modified: Controllers now have a GetByName and GetByType which can be overriden and are used by views.
- Modified: SceneManagers no longer initialize single instance types.  So a at least one view should check InitializeViewModel in the inspector

1.04 5/4/2014
IMPORTANT!!!: Since we re-released the source code you will need to reconnect all GameManagers in you're scene.
IMPORTANT!!!: Make sure you delete the uFrameComplete folder then re-import.
- Note: Refactoring doesn't apply to changing type parameters on commands or enum items yet.
- Note: Now source code for uFrame is available again
- Added: New refactoring library added for renaming anything
- Added: New skin for a better look and feel
- Added: Panning via thie middle mouse button
- Added: Zooming (This will be improved in later builds)
- Modified: SceneManager Settings files are now partial so they can be extended.
- Modified: You no longer have to use shift for multi-selection.
- Fixed: A lot of ViewComponent link issues
- Fixed: Unity Free skin where things were hard to see
- Fixed: Where deleting a view wouldn't delete the view file
- Fixed: Collapsed states not being persisted correctly between sub diagrams
- Fixed: Switches to LoadLevelAdditive if you don't have a pro license.
- Fixed: iOS Mobile issues
- Fixed: Diagram Loading issues causing unity to crash or not able to load when Unity starts
- Fixed: Collapsing link issue

1.0Beta 4/27/2014
- Added an all new element designer that generates SceneManagers(GameTypes), Controllers, ViewModels, Views, & View Components.
- Add a whole new action behaviour system called uBehaviours for quick one-off components.
- Integration of uBehaviours and the new diagram system.
- Rename GameType to SceneManager.
- Watch ViewModels from the inspector
- Initialize ViewModels from the inspector (Element Designer generates this)
- Automatic Bindings generation.
- Controllers have been removed from the scene the Element designer will automatically Dependency inject them.
- Simplified bindings via the Element Designer
- Updated Checkers Demo (To Demonstrate Diagrams and uBehaviours)
- Update FPS Demo
- Loading screen changed to just a view in a scene and nothing more.
- Dependency containers are no longer cleared when moving from scene to scene.
- Tons of other fixes and improvements.

0.99r1 3/8/2014
- Added IBindingProvider interface so that other components can directly connect to a View Components Bind & Unbind methods.
- Removed the class constraints on Container.Resolve & Container.RegisterInstance.
- Add a User Submitted Cheat Sheet diagram.
- Other Minor Fixes/Improvements.

0.99 2/4/2014
- Added A single ViewModel to View Binding via 
	// View Class
	public SingleView ChildView {get;set;}
	// In Bind method
	this.BindToView(() => Model._SingleViewModelProperty, v => ChildView = v, () => ChildView);
- Bug Fix: Now in objects where the Inject attribute is used and there isn't a registered instance it will keep the value at null.
- Important Note: Now when commands are Executed by default it passes the viewmodel along with it. 

Used to look like this.-------------
    public virtual TWeaponViewModel Create<TWeaponViewModel>() where TWeaponViewModel : FPSWeaponViewModel, new()
    {
        var fpsWeapon = new TWeaponViewModel();
        fpsWeapon.NextZoomCommand = new CommandWith<FPSWeaponViewModel>(NextZoom);
        fpsWeapon.FireCommand = new YieldCommandWith<FPSWE>(() =>
        {
            fpsWeapon.Spread += _SpreadMultiplier;

            if (fpsWeapon.IsFiring)
                return null;

            return Fire(fpsWeapon);
        });
        fpsWeapon.RecoilAmount = _RecoilSpeed;
        fpsWeapon.ReloadCommand = new YieldCommand(() => { return Reload(fpsWeapon); });
        fpsWeapon.Spread = _Spread;
        fpsWeapon.Ammo = _RoundSize * 3;
        fpsWeapon.State = FPSWeaponState.Active;
        return fpsWeapon;
    }

You can now do this--------------------------------------------------------------
    public virtual TWeaponViewModel Create<TWeaponViewModel>() where TWeaponViewModel : FPSWeaponViewModel, new()
    {
        return new TWeaponViewModel
        {
            NextZoomCommand = new CommandWith<FPSWeaponViewModel>(NextZoom),
            FireCommand = new YieldCommandWith<FPSWeaponViewModel>(FireWeapon),
            RecoilAmount = _RecoilSpeed,
            ReloadCommand = new YieldCommandWith<FPSWeaponViewModel>(Reload),
            Spread = _Spread,
            Ammo = _RoundSize*3,
            State = FPSWeaponState.Active
        };
    }

0.98r1 2/2/2014
- Bug Fix: Child Views will sometimes have a Model that is null causing an exception in Bind.
- Examples: Updated some example code on the FPS Game.

0.98 2/1/2014
- Updated View Editor.
- Fixed Templates for SceneManager.
- Seperated templates for Generating a single file or generating multiple files via "New Element Structure" or "View Tools"
- Generating a View Prefab now automatically sets the "ViewModel From: to Controller and method that is generated.
- Fixed Window Hanging on Code Generation.
- Modified the Level Loading Algorithm to be more direct of the value passed in UpgradeProgressDelegate. (Thanks killamaaki)
- Fixed Controller Method Issue on Views where two Views in the scene caused a controller not to register.

0.97 1/30/2014
- Minor Editor Bug Fixes

0.96 1/29/2014
- InitializeModel method was renamed to InitializeViewModel ( BREAKING CHANGE APPLY RENAME WHERE USED! )
- Game class renamed to SceneManager ( Marked obsolete to not breaking anything )
- vmp snippet fixed and creates properties named _NameProperty now instead of _Name
- Added the ability to specify a controller & method that will be used to create a ViewModel when View's already exist in a scene.
- Fixed a GUI Styles Warning ( Thanks Shawn! )

0.95 1/24/2014
- Added FPS Demo
- You can now use ExecuteCommand to execute commands with a parameter
- Added the ability to set parameters on command bindings.
- Added the ability for collision event bindings to be subscribed to with a GameObject as the parameter of the collider.
- Fixed Automatic controller injection where a controller that has multiple other controller properties being injected.

0.92 1/19/2014
	- Fixed a Property Binding Component where the Model Property wouldn't be set until the target component was set.

0.9 1/12/2014
	- Initial Release

For Support Questions Contact:

Micah Osborne
invertgamestudios@gmail.com