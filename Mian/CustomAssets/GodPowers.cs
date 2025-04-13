namespace Better_Loving.Mian.CustomAssets
{
    public class GodPowers
    {
        private static Actor _selectedActorA;
        private static Actor _selectedActorB;
        public static void Init()
        {
                        var forceLover = new GodPower
            {
                id = "forceLover",
                name = "ForceLover",
                force_map_mode = MetaType.Unit,
                path_icon = "god_powers/force_lover", // temporary icon!
                can_drag_map = true,
                type = PowerActionType.PowerSpecial,
                select_button_action = powerId => 
                {
                    WorldTip.showNow("love_selected", pPosition: "top");
                    _selectedActorA = null;
                    _selectedActorB = null;
                    return false;
                },
                click_special_action = (pTile, powerId) =>
                {
                    var pActor = pTile != null ? ActionLibrary.getActorFromTile(pTile) : World.world.getActorNearCursor();
                    if (pActor == null)
                        return false;

                    if (_selectedActorA == null)
                    {
                        _selectedActorA = pActor;
                        ActionLibrary.showWhisperTip("love_selected_first");
                        return false;
                    } 
                    
                    if (_selectedActorB == null && pActor == _selectedActorA)
                    {
                        ActionLibrary.showWhisperTip("love_cancelled");
                        _selectedActorA = null;
                        _selectedActorB = null;
                        return false;
                    }

                    if (_selectedActorA.lover == pActor)
                    {
                        ActionLibrary.showWhisperTip("love_cancelled");
                        _selectedActorA = null;
                        _selectedActorB = null;
                        return false;
                    }
                
                    ActionLibrary.showWhisperTip("love_successful");
                    _selectedActorB = pActor;
                    _selectedActorA.becomeLoversWith(_selectedActorB);
                    _selectedActorA = null;
                    _selectedActorB = null;
                
                    return true;
                },
                tester_enabled = false
            };
            AssetManager.powers.add(forceLover);
            
            // TODO: finish this when NML unscuffs their UI
            // TabManager.TabOther.AddPowerButton("Default", PowerButtonCreator.CreateGodPowerButton("forceLover", 
            // SpriteTextureLoader.getSprite("ui/Icons/god_powers/iconForceLover")));
        }
    }
}