/// <summary> KDH : Resources 폴더 경로에 있는 사운드를 가져오기 위한 키 </summary>
public abstract class SoundKey
{
    public static class Path
    {
        public const string Sound = "Sound/";
        public const string SoundMixer = Sound + "Mixer/SoundMixer";

        public const string Bgm = Sound + "Bgm/";
        public const string Sfx = Sound + "Sfx/";
    }

    public static class Name
    {
        #region ===== 사운드 =====
        
        public enum BgmType
        {
        }

        public enum SfxType
        {
            ButtonSound_1, // ButtonsSound 고정
            ButtonSound_2, // ButtonsSound 고정
            ButtonSound_3, // ButtonsSound 고정
            ButtonSound_4, // ButtonsSound 고정

            ChimeConfirm,
            ChimeCancel,
            ChimePopup,
            CustomerEnter,
            CustomerExit,
            
            EnemyChaseStart,
            
            FishPickup,
            
            ItemBottle,
            ItemTimer,
            
            Reputation,
            
            StartBusiness,
            StealthArrest,
            StealthEscape,
            
            GameOver,
            
            QuestClear,
            
            TutorialOpen,
        }
        
        #endregion ===== 사운드 =====
    }
}