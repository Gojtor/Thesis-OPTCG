using System.Runtime.Serialization;

namespace TCGSim.CardResources
{

    public enum CharacterType
    {
        [EnumMember(Value = "NONE")]
        NONE,

        [EnumMember(Value = "The Four EmperorsBig Mom Pirates")]
        TheFourEmperorsBigMomPirates,

        [EnumMember(Value = "Big Mom Pirates")]
        BigMomPirates,

        [EnumMember(Value = "Navy")]
        Navy,

        [EnumMember(Value = "Straw Hat Crew")]
        StrawHatCrew,

        [EnumMember(Value = "The Seven Warlords of the SeaBlackbeard Pirates")]
        TheSevenWarlordsOfTheSeaBlackbeardPirates,

        [EnumMember(Value = "The Seven Warlords of the SeaKuja Pirates")]
        TheSevenWarlordsOfTheSeaKujaPirates,

        [EnumMember(Value = "The Seven Warlords of the SeaBuggy's Delivery")]
        TheSevenWarlordsOfTheSeaBuggysDelivery,

        [EnumMember(Value = "The Seven Warlords of the SeaHeart Pirates")]
        TheSevenWarlordsOfTheSeaHeartPirates,

        [EnumMember(Value = "The Seven Warlords of the SeaBaroque Works")]
        TheSevenWarlordsOfTheSeaBaroqueWorks,

        [EnumMember(Value = "FILMSupernovasStraw Hat Crew")]
        FILMSupernovasStrawHatCrew,

        [EnumMember(Value = "FILMThe Four EmperorsRed-Haired Pirates")]
        FILMTheFourEmperorsRedHairedPirates,

        [EnumMember(Value = "FILMBig Mom Pirates")]
        FILMBigMomPirates,

        [EnumMember(Value = "FILM")]
        FILM,

        [EnumMember(Value = "Whitebeard Pirates")]
        WhitebeardPirates,

        [EnumMember(Value = "The Four EmperorsWhitebeard Pirates")]
        TheFourEmperorsWhitebeardPirates,

        [EnumMember(Value = "Bowin Island")]
        BowinIsland,

        [EnumMember(Value = "Sky Island")]
        SkyIsland,

        [EnumMember(Value = "AnimalStraw Hat Crew")]
        AnimalStrawHatCrew,

        [EnumMember(Value = "Fish-ManThe Sun Pirates")]
        FishManTheSunPirates,

        [EnumMember(Value = "Goa Kingdom")]
        GoaKingdom,

        [EnumMember(Value = "Punk HazardStraw Hat Crew")]
        PunkHazardStrawHatCrew,

        [EnumMember(Value = "DressrosaRevolutionary Army")]
        DressrosaRevolutionaryArmy,

        [EnumMember(Value = "Land of Wano")]
        LandOfWano,

        [EnumMember(Value = "SupernovasStraw Hat Crew")]
        SupernovasStrawHatCrew,

        [EnumMember(Value = "Windmill Village")]
        WindmillVillage,

        [EnumMember(Value = "The Four EmperorsRed-Haired Pirates")]
        TheFourEmperorsRedHairedPirates,

        [EnumMember(Value = "Mountain Bandits")]
        MountainBandits,

        [EnumMember(Value = "Revolutionary Army")]
        RevolutionaryArmy,

        [EnumMember(Value = "East Blue")]
        EastBlue,

        [EnumMember(Value = "The Flying Fish Riders")]
        TheFlyingFishRiders,

        [EnumMember(Value = "Impel DownRevolutionary Army")]
        ImpelDownRevolutionaryArmy,

        [EnumMember(Value = "Animal")]
        Animal,

        [EnumMember(Value = "Muggy KingdomThriller Bark Pirates")]
        MuggyKingdomThrillerBarkPirates,

        [EnumMember(Value = "AnimalMuggy Kingdom")]
        AnimalMuggyKingdom,

        [EnumMember(Value = "The Seven Warlords of the SeaMuggy Kingdom")]
        TheSevenWarlordsOfTheSeaMuggyKingdom,

        [EnumMember(Value = "Frost Moon Village")]
        FrostMoonVillage,

        [EnumMember(Value = "MusicFILM")]
        MusicFILM,

        [EnumMember(Value = "Kid Pirates")]
        KidPirates,

        [EnumMember(Value = "MinksHeart Pirates")]
        MinksHeartPirates,

        [EnumMember(Value = "Heart Pirates")]
        HeartPirates,

        [EnumMember(Value = "Fish-ManStraw Hat Crew")]
        FishManStrawHatCrew,

        [EnumMember(Value = "Land of WanoStraw Hat Crew")]
        LandOfWanoStrawHatCrew,

        [EnumMember(Value = "Land of WanoWhitebeard Pirates")]
        LandOfWanoWhitebeardPirates,

        [EnumMember(Value = "Land of WanoKouzuki Clan")]
        LandOfWanoKouzukiClan,

        [EnumMember(Value = "The Four EmperorsAnimal Kingdom Pirates")]
        TheFourEmperorsAnimalKingdomPirates,

        [EnumMember(Value = "Animal Kingdom Pirates")]
        AnimalKingdomPirates,

        [EnumMember(Value = "Former Baroque Works")]
        FormerBaroqueWorks,

        [EnumMember(Value = "Alabasta")]
        Alabasta,

        [EnumMember(Value = "Merfolk")]
        Merfolk,

        [EnumMember(Value = "Red-Haired Pirates")]
        RedHairedPirates,

        [EnumMember(Value = "Big Mom PiratesHomies")]
        BigMomPiratesHomies,

        [EnumMember(Value = "MinksBig Mom Pirates")]
        MinksBigMomPirates,

        [EnumMember(Value = "FILMThe Pirates Fest")]
        FILMThePiratesFest,

        [EnumMember(Value = "FILMGolden Lion Pirates")]
        FILMGoldenLionPirates,

        [EnumMember(Value = "FILMScientistGolden Lion Pirates")]
        FILMScientistGoldenLionPirates,

        [EnumMember(Value = "FILM海賊万博")]
        FILM海賊万博,

        [EnumMember(Value = "FILMNeo Navy")]
        FILMNeoNavy,

        [EnumMember(Value = "FILMGrantesoro")]
        FILMGrantesoro,

        [EnumMember(Value = "FILMAnimalGolden Lion Pirates")]
        FILMAnimalGoldenLionPirates,

        [EnumMember(Value = "四皇百獣海賊団")]
        四皇百獣海賊団,

        [EnumMember(Value = "NavyDrake PiratesAnimal Kingdom Pirates")]
        NavyDrakePiratesAnimalKingdomPirates,

        [EnumMember(Value = "Animal Kingdom PiratesSmile")]
        AnimalKingdomPiratesSmile,

        [EnumMember(Value = "王下七武海九蛇海賊団")]
        王下七武海九蛇海賊団,

        [EnumMember(Value = "The Seven Warlords of the SeaRevolutionary Army")]
        TheSevenWarlordsOfTheSeaRevolutionaryArmy,

        [EnumMember(Value = "Biological WeaponNavy")]
        BiologicalWeaponNavy,

        [EnumMember(Value = "生物兵器海軍")]
        生物兵器海軍,

        [EnumMember(Value = "The Seven Warlords of the SeaDonquixote Pirates")]
        TheSevenWarlordsOfTheSeaDonquixotePirates,

        [EnumMember(Value = "Fish-ManThe Seven Warlords of the SeaThe Sun Pirates")]
        FishManTheSevenWarlordsOfTheSeaTheSunPirates,

        [EnumMember(Value = "The Seven Warlords of the Sea")]
        TheSevenWarlordsOfTheSea,

        [EnumMember(Value = "The Seven Warlords of the SeaThriller Bark Pirates")]
        TheSevenWarlordsOfTheSeaThrillerBarkPirates,

        [EnumMember(Value = "王下七武海B・W")]
        王下七武海B_W,

        [EnumMember(Value = "SupernovasHawkins Pirates")]
        SupernovasHawkinsPirates,

        [EnumMember(Value = "SupernovasKid Pirates")]
        SupernovasKidPirates,

        [EnumMember(Value = "SupernovasHeart Pirates")]
        SupernovasHeartPirates,

        [EnumMember(Value = "NavySupernovasDrake Pirates")]
        NavySupernovasDrakePirates,

        [EnumMember(Value = "超新星キッド海賊団")]
        超新星キッド海賊団,

        [EnumMember(Value = "SupernovasOn-Air Pirates")]
        SupernovasOnAirPirates,

        [EnumMember(Value = "SupernovasBonney Pirates")]
        SupernovasBonneyPirates,

        [EnumMember(Value = "SupernovasFiretank Pirates")]
        SupernovasFiretankPirates,

        [EnumMember(Value = "SupernovasFallen Monk Pirates")]
        SupernovasFallenMonkPirates,

        [EnumMember(Value = "Firetank Pirates")]
        FiretankPirates,

        [EnumMember(Value = "AnimalAlabasta")]
        AnimalAlabasta,

        [EnumMember(Value = "ODYSSEY")]
        ODYSSEY,

        [EnumMember(Value = "超新星麦わらの一味")]
        超新星麦わらの一味,

        [EnumMember(Value = "白ひげ海賊団")]
        白ひげ海賊団,

        [EnumMember(Value = "ドレスローザ革命軍")]
        ドレスローザ革命軍,

        [EnumMember(Value = "Monsters")]
        Monsters,

        [EnumMember(Value = "Minks")]
        Minks,

        [EnumMember(Value = "Kingdom of GERMAThe Vinsmoke Family")]
        KingdomOfGERMATheVinsmokeFamily,

        [EnumMember(Value = "Fish-Man")]
        Fish_Man,

        [EnumMember(Value = "Fish-ManMerfolkNew Fish-Man Pirates")]
        Fish_ManMerfolkNewFish_ManPirates,

        [EnumMember(Value = "FILMStraw Hat Crew")]
        FILMStrawHatCrew,

        [EnumMember(Value = "Fish-ManArlong Pirates")]
        Fish_ManArlongPirates,

        [EnumMember(Value = "The Four EmperorsStraw Hat Crew")]
        TheFourEmperorsStrawHatCrew,

        [EnumMember(Value = "Bellamy Pirates")]
        BellamyPirates,

        [EnumMember(Value = "The Vinsmoke Family")]
        TheVinsmokeFamily,

        [EnumMember(Value = "FILMSupernovasBarto Club")]
        FILMSupernovasBartoClub,

        [EnumMember(Value = "FILMRed-Haired Pirates")]
        FILMRedHairedPirates,

        [EnumMember(Value = "FILMNavy")]
        FILMNavy,

        [EnumMember(Value = "FILMMinksHeart Pirates")]
        FILMMinksHeartPirates,

        [EnumMember(Value = "FILMSupernovasHeart Pirates")]
        FILMSupernovasHeartPirates,

        [EnumMember(Value = "FILMJellyfish Pirates")]
        FILMJellyfishPirates,

        [EnumMember(Value = "King of the PiratesRoger Pirates")]
        KingOfThePiratesRogerPirates,

        [EnumMember(Value = "Ohara")]
        Ohara,

        [EnumMember(Value = "MinksRevolutionary Army")]
        MinksRevolutionaryArmy,

        [EnumMember(Value = "GiantRevolutionary Army")]
        GiantRevolutionaryArmy,

        [EnumMember(Value = "EggheadStraw Hat Crew")]
        EggheadStrawHatCrew,

        [EnumMember(Value = "AnimalSky Island")]
        AnimalSkyIsland,

        [EnumMember(Value = "OharaGiantNavy")]
        OharaGiantNavy,

        [EnumMember(Value = "Blackbeard Pirates")]
        BlackbeardPirates,

        [EnumMember(Value = "Peachbeard PiratesBlackbeard Pirates Allies")]
        PeachbeardPiratesBlackbeardPiratesAllies,

        [EnumMember(Value = "The Four EmperorsBlackbeard Pirates")]
        TheFourEmperorsBlackbeardPirates,

        [EnumMember(Value = "AnimalBlackbeard Pirates")]
        AnimalBlackbeardPirates,

        [EnumMember(Value = "Thriller Bark Pirates")]
        ThrillerBarkPirates,

        [EnumMember(Value = "Cross Guild")]
        CrossGuild,

        [EnumMember(Value = "Buggy Pirates")]
        BuggyPirates,

        [EnumMember(Value = "Cross GuildFormer Baroque Works")]
        CrossGuildFormerBaroqueWorks,

        [EnumMember(Value = "AnimalCross Guild")]
        AnimalCrossGuild,

        [EnumMember(Value = "The Four EmperorsCross Guild")]
        TheFourEmperorsCrossGuild,

        [EnumMember(Value = "ODYSSEYStraw Hat Crew")]
        ODYSSEYStrawHatCrew,

        [EnumMember(Value = "ODYSSEYSupernovasStraw Hat Crew")]
        ODYSSEYSupernovasStrawHatCrew,

        [EnumMember(Value = "ODYSSEYCP9")]
        ODYSSEYCP9,

        [EnumMember(Value = "ODYSSEYWhitebeard Pirates")]
        ODYSSEYWhitebeardPirates,

        [EnumMember(Value = "ODYSSEYNavyDonquixote Pirates")]
        ODYSSEYNavyDonquixotePirates,

        [EnumMember(Value = "ODYSSEYThe Seven Warlords of the SeaDonquixote Pirates")]
        ODYSSEYTheSevenWarlordsOfTheSeaDonquixotePirates,

        [EnumMember(Value = "ODYSSEYSupernovasHeart Pirates")]
        ODYSSEYSupernovasHeartPirates,

        [EnumMember(Value = "ODYSSEYAnimalStraw Hat Crew")]
        ODYSSEYAnimalStrawHatCrew,

        [EnumMember(Value = "ODYSSEYRevolutionary Army")]
        ODYSSEYRevolutionaryArmy,

        [EnumMember(Value = "ODYSSEYNavy")]
        ODYSSEYNavy,

        [EnumMember(Value = "ODYSSEYThe Seven Warlords of the SeaBaroque Works")]
        ODYSSEYTheSevenWarlordsOfTheSeaBaroqueWorks,

        [EnumMember(Value = "AnimalRed-Haired Pirates")]
        AnimalRedHairedPirates,

        [EnumMember(Value = "Roger Pirates")]
        Roger,

        [EnumMember(Value = "The Four EmperorsAnimal Kingdom PiratesBig Mom Pirates")]
        TheFourEmperorsAnimalKingdomPiratesBigMomPirates,

        [EnumMember(Value = "Former Roger Pirates")]
        FormerRogerPirates,

        [EnumMember(Value = "Sky IslandShandian Warrior")]
        SkyIslandShandianWarrior,

        [EnumMember(Value = "EggheadSeraphim")]
        EggheadSeraphim,

        [EnumMember(Value = "JayaBotanist")]
        JayaBotanist,

        [EnumMember(Value = "JayaMonkey Mountain Alliance")]
        JayaMonkeyMountainAlliance,

        [EnumMember(Value = "EggheadBonney Pirates")]
        EggheadBonneyPirates,

        [EnumMember(Value = "AnimalJayaSky Island")]
        AnimalJayaSkyIsland,

        [EnumMember(Value = "JayaSky IslandShandian Warrior")]
        JayaSkyIslandShandianWarrior,

        [EnumMember(Value = "Animal Kingdom PiratesFormer CP9")]
        AnimalKingdomPiratesFormerCP9,

        [EnumMember(Value = "Animal Kingdom PiratesSMILE")]
        AnimalKingdomPiratesSMILE,

        [EnumMember(Value = "Animal Kingdom PiratesHawkins Pirates")]
        AnimalKingdomPiratesHawkinsPirates,

        [EnumMember(Value = "Animal Kingdom PiratesOn-Air Pirates")]
        AnimalKingdomPiratesOnAirPirates,

        [EnumMember(Value = "CP0")]
        CP0,

        [EnumMember(Value = "Former Rocks PiratesAnimal Kingdom Pirates")]
        FormerRocksPiratesAnimalKingdomPirates,

        [EnumMember(Value = "Former Rocks PiratesBig Mom Pirates")]
        FormerRocksPiratesBigMomPirates,

        [EnumMember(Value = "LunarianAnimal Kingdom Pirates")]
        LunarianAnimalKingdomPirates,

        [EnumMember(Value = "Former Whitebeard Pirates")]
        FormerWhitebeardPirates,

        [EnumMember(Value = "Former Rocks Pirates")]
        FormerRocksPirates,

        [EnumMember(Value = "Fish-ManWhitebeard Pirates")]
        FishManWhitebeardPirates,

        [EnumMember(Value = "Amazon Lily")]
        AmazonLily,

        [EnumMember(Value = "MinksThe Akazaya Nine")]
        MinksTheAkazayaNine,

        [EnumMember(Value = "Drum Kingdom")]
        DrumKingdom,

        [EnumMember(Value = "AnimalDrum KingdomStraw Hat Crew")]
        AnimalDrumKingdomStrawHatCrew,

        [EnumMember(Value = "AnimalDrum Kingdom")]
        AnimalDrumKingdom,

        [EnumMember(Value = "FILMDrum Kingdom")]
        FILMDrumKingdom,

        [EnumMember(Value = "Egghead")]
        Egghead,

        [EnumMember(Value = "Land of WanoThe Akazaya Nine")]
        LandOfWanoTheAkazayaNine,

        [EnumMember(Value = "ScientistEgghead")]
        ScientistEgghead,

        [EnumMember(Value = "The Four EmperorsEggheadStraw Hat Crew")]
        TheFourEmperorsEggheadStrawHatCrew,

        [EnumMember(Value = "AnimalEggheadStraw Hat Crew")]
        AnimalEggheadStrawHatCrew,

        [EnumMember(Value = "Fish-ManEggheadStraw Hat Crew")]
        FishManEggheadStrawHatCrew,

        [EnumMember(Value = "CP9")]
        CP9,

        [EnumMember(Value = "Journalist")]
        Journalist,

        [EnumMember(Value = "AnimalCP0")]
        AnimalCP0,

        [EnumMember(Value = "World Government")]
        WorldGovernment,

        [EnumMember(Value = "Thriller Bark PiratesFormer Rocks Pirates")]
        ThrillerBarkPiratesFormerRocksPirates,

        [EnumMember(Value = "Foxy Pirates")]
        FoxyPirates,

        [EnumMember(Value = "The Four EmperorsLand of Wano")]
        TheFourEmperorsLandOfWano,

        [EnumMember(Value = "AnimalFoxy Pirates")]
        AnimalFoxyPirates,

        [EnumMember(Value = "GiantFish-ManFoxy Pirates")]
        GiantFishManFoxyPirates,

        [EnumMember(Value = "Long Ring Long Land")]
        LongRingLongLand,

        [EnumMember(Value = "Fish-ManFoxy Pirates")]
        FishManFoxyPirates,

        [EnumMember(Value = "Kuja Pirates")]
        KujaPirates,

        [EnumMember(Value = "Weevil's Mother")]
        WeevilsMother,

        [EnumMember(Value = "AnimalKuja Pirates")]
        AnimalKujaPirates,

        [EnumMember(Value = "SupernovasBarto Club")]
        SupernovasBartoClub,

        [EnumMember(Value = "SupernovasCaribou Pirates")]
        SupernovasCaribouPirates,

        [EnumMember(Value = "Foolshout Island")]
        FoolshoutIsland,

        [EnumMember(Value = "MerfolkThe Sun Pirates")]
        MerfolkTheSunPirates,

        [EnumMember(Value = "Lulucia Kingdom")]
        LuluciaKingdom,

        [EnumMember(Value = "Spade Pirates")]
        SpadePirates,

        [EnumMember(Value = "Goa KingdomBluejam Pirates")]
        GoaKingdomBluejamPirates,

        [EnumMember(Value = "MinksLand of WanoThe Akazaya Nine")]
        MinksLandOfWanoTheAkazayaNine,

        [EnumMember(Value = "Fish-ManLand of WanoThe Akazaya Nine")]
        FishManLandOfWanoTheAkazayaNine,

        [EnumMember(Value = "Former Rumbar Pirates")]
        FormerRumbarPirates,

        [EnumMember(Value = "DressrosaHapposui Army")]
        DressrosaHapposuiArmy,

        [EnumMember(Value = "GiantThriller Bark Pirates")]
        GiantThrillerBarkPirates,

        [EnumMember(Value = "Kingdom of GERMA")]
        KingdomOfGERMA,

        [EnumMember(Value = "The Vinsmoke FamilyGERMA 66")]
        TheVinsmokeFamilyGERMA66,

        [EnumMember(Value = "SupernovasKid PiratesSMILE")]
        SupernovasKidPiratesSMILE,

        [EnumMember(Value = "FILMCrown Island")]
        FILMCrownIsland,

        [EnumMember(Value = "FILMEldoraggo Crew")]
        FILMEldoraggoCrew,

        [EnumMember(Value = "The Vinsmoke FamilyKingdom of GERMA")]
        TheVinsmokeFamilyKingdomOfGERMA,

        [EnumMember(Value = "DressrosaStraw Hat Crew")]
        DressrosaStrawHatCrew,

        [EnumMember(Value = "GiantNavy")]
        GiantNavy,

        [EnumMember(Value = "Fish-Man Island")]
        FishManIsland,

        [EnumMember(Value = "Fish-ManNew Fish-Man Pirates")]
        FishManNewFishManPirates,

        [EnumMember(Value = "Fish-ManFlying Pirates")]
        FishManFlyingPirates,

        [EnumMember(Value = "Land of WanoThriller Bark Pirates")]
        LandOfWanoThrillerBarkPirates,

        [EnumMember(Value = "Fish-ManFormer Arlong Pirates")]
        FishManFormerArlongPirates,

        [EnumMember(Value = "Gyro Pirates")]
        GyroPirates,

        [EnumMember(Value = "Fish-ManEast BlueArlong Pirates")]
        FishManEastBlueArlongPirates,

        [EnumMember(Value = "FILMRevolutionary Army")]
        FILMRevolutionaryArmy,

        [EnumMember(Value = "FILMOmatsuri Island")]
        FILMOmatsuriIsland,

        [EnumMember(Value = "FILMMecha Island")]
        FILMMechaIsland,

        [EnumMember(Value = "FILMTrump Pirates")]
        FILMTrumpPirates,

        [EnumMember(Value = "FILMShipbuilding Town")]
        FILMShipbuildingTown,

        [EnumMember(Value = "FILMMugiwara Chase")]
        FILMMugiwaraChase,

        [EnumMember(Value = "FILMAsuka Island")]
        FILMAsukaIsland,

        [EnumMember(Value = "FILMFormer NavyGasparde Pirates")]
        FILMFormerNavyGaspardePirates,

        [EnumMember(Value = "The Moon")]
        TheMoon,

        [EnumMember(Value = "Sky IslandVassals")]
        SkyIslandVassals,

        [EnumMember(Value = "Mary Geoise")]
        MaryGeoise,

        [EnumMember(Value = "Celestial Dragons")]
        CelestialDragons,

        [EnumMember(Value = "The TontattasDressrosa")]
        TheTontattasDressrosa,

        [EnumMember(Value = "Dressrosa")]
        Dressrosa,

        [EnumMember(Value = "DressrosaBeautiful Pirates")]
        DressrosaBeautifulPirates,

        [EnumMember(Value = "Kingdom of ProdenceDressrosa")]
        KingdomOfProdenceDressrosa,

        [EnumMember(Value = "Baroque Works")]
        BaroqueWorks,

        [EnumMember(Value = "Former Navy")]
        FormerNavy,

        [EnumMember(Value = "GiantAnimal Kingdom Pirates")]
        GiantAnimalKingdomPirates,

        [EnumMember(Value = "Donquixote Pirates")]
        DonquixotePirates,

        [EnumMember(Value = "NavyDonquixote Pirates")]
        NavyDonquixotePirates,

        [EnumMember(Value = "Fish-ManRevolutionary Army")]
        FishManRevolutionaryArmy,

        [EnumMember(Value = "Whole Cake Island")]
        WholeCakeIsland,

        [EnumMember(Value = "ビッグ・マム海賊団")]
        ビッグマム海賊団,

        [EnumMember(Value = "The House of Lambs")]
        TheHouseOfLambs,

        [EnumMember(Value = "ファイアタンク海賊団")]
        ファイアタンク海賊団,

        [EnumMember(Value = "Land of WanoThe Four EmperorsBig Mom Pirates")]
        LandOfWanoTheFourEmperorsBigMomPirates,

        [EnumMember(Value = "DressrosaBarto Club")]
        DressrosaBartoClub,

        [EnumMember(Value = "GiantDressrosaNew Giant Pirates")]
        GiantDressrosaNewGiantPirates,

        [EnumMember(Value = "DressrosaHeart Pirates")]
        DressrosaHeartPirates,

        [EnumMember(Value = "DressrosaYonta Maria Fleet")]
        DressrosaYontaMariaFleet,

        [EnumMember(Value = "GiantWorld Government")]
        GiantWorldGovernment,

        [EnumMember(Value = "AnimalBaroque Works")]
        AnimalBaroqueWorks,

        [EnumMember(Value = "AnimalWater Seven")]
        AnimalWaterSeven,

        [EnumMember(Value = "Water SevenThe Franky Family")]
        WaterSevenTheFrankyFamily,

        [EnumMember(Value = "Fish-ManWater Seven")]
        FishManWaterSeven,

        [EnumMember(Value = "Water SevenGalley-La Company")]
        WaterSevenGalleyLaCompany,

        [EnumMember(Value = "PlagueAnimal Kingdom Pirates")]
        PlagueAnimalKingdomPirates,

        [EnumMember(Value = "East BlueFormer Navy")]
        EastBlueFormerNavy,

        [EnumMember(Value = "East BlueBlack Cat Pirates")]
        EastBlueBlackCatPirates,

        [EnumMember(Value = "AlabastaStraw Hat Crew")]
        AlabastaStrawHatCrew,

        [EnumMember(Value = "AnimalAlabastaStraw Hat Crew")]
        AnimalAlabastaStrawHatCrew,

        [EnumMember(Value = "Sniper Island")]
        SniperIsland,

        [EnumMember(Value = "四皇ビッグ・マム海賊団")]
        四皇ビッグマム海賊団,

        [EnumMember(Value = "CP7")]
        CP7,

        [EnumMember(Value = "CP6")]
        CP6,

        [EnumMember(Value = "Water SevenStraw Hat Crew")]
        WaterSevenStrawHatCrew,

        [EnumMember(Value = "Impel DownJailer Beast")]
        ImpelDownJailerBeast,

        [EnumMember(Value = "MerfolkWater Seven")]
        MerfolkWaterSeven,

        [EnumMember(Value = "East BlueStraw Hat Crew")]
        EastBlueStrawHatCrew,

        [EnumMember(Value = "East BlueKrieg Pirates")]
        EastBlueKriegPirates,

        [EnumMember(Value = "AnimalEast Blue")]
        AnimalEastBlue,

        [EnumMember(Value = "East BlueBuggy Pirates")]
        EastBlueBuggyPirates,

        [EnumMember(Value = "East BlueArlong Pirates")]
        EastBlueArlongPirates,

        [EnumMember(Value = "East BlueAlvida Pirates")]
        EastBlueAlvidaPirates,

        [EnumMember(Value = "海軍")]
        海軍,

        [EnumMember(Value = "Impel Down")]
        ImpelDown,

        [EnumMember(Value = "AnimalImpel Down")]
        AnimalImpelDown,

        [EnumMember(Value = "World Pirates")]
        WorldPirates,

        [EnumMember(Value = "Impel DownStraw Hat Crew")]
        ImpelDownStrawHatCrew,

        [EnumMember(Value = "Fish-ManImpel Down")]
        FishManImpelDown,

        [EnumMember(Value = "Impel DownFormer Baroque Works")]
        ImpelDownFormerBaroqueWorks,

        [EnumMember(Value = "Impel DownThe Seven Warlords of the SeaKuja Pirates")]
        ImpelDownTheSevenWarlordsOfTheSeaKujaPirates,

        [EnumMember(Value = "Impel DownBuggy Pirates")]
        ImpelDownBuggyPirates,

        [EnumMember(Value = "FILM麦わらの一味")]
        FILM麦わらの一味,

        [EnumMember(Value = "FILMAnimalStraw Hat Crew")]
        FILMAnimalStrawHatCrew,

        [EnumMember(Value = "FILMFish-ManStraw Hat Crew")]
        FILMFishManStrawHatCrew,

        [EnumMember(Value = "GiantWhitebeard Pirates Allies")]
        GiantWhitebeardPiratesAllies,

        [EnumMember(Value = "Whitebeard Pirates Allies")]
        WhitebeardPiratesAllies,

        [EnumMember(Value = "山賊")]
        山賊,

        [EnumMember(Value = "四皇白ひげ海賊団")]
        四皇白ひげ海賊団,

        [EnumMember(Value = "海軍ドレーク海賊団百獣海賊団")]
        海軍ドレーク海賊団百獣海賊団,

        [EnumMember(Value = "Land of WanoAnimal Kingdom Pirates")]
        LandOfWanoAnimalKingdomPirates,

        [EnumMember(Value = "Land of WanoKurozumi Clan")]
        LandOfWanoKurozumiClan,

        [EnumMember(Value = "Donquixote PiratesPunk Hazard")]
        DonquixotePiratesPunkHazard,

        [EnumMember(Value = "Punk Hazard")]
        PunkHazard,

        [EnumMember(Value = "Biological WeaponPunk Hazard")]
        BiologicalWeaponPunkHazard,

        [EnumMember(Value = "ScientistPunk Hazard")]
        ScientistPunkHazard,

        [EnumMember(Value = "Krieg Pirates")]
        KriegPirates,

        [EnumMember(Value = "NavyDonquixote PiratesPunk Hazard")]
        NavyDonquixotePiratesPunkHazard,

        [EnumMember(Value = "ワノ国光月家")]
        ワノ国光月家,

        [EnumMember(Value = "SupernovasNavyDrake Pirates")]
        SupernovasNavyDrakePirates,

        [EnumMember(Value = "Land of WanoFormer Whitebeard Pirates")]
        LandOfWanoFormerWhitebeardPirates,

        [EnumMember(Value = "麦わらの一味")]
        麦わらの一味,

        [EnumMember(Value = "SupernovasBeautiful Pirates")]
        SupernovasBeautifulPirates,

        [EnumMember(Value = "GiantNew Giant Pirates")]
        GiantNewGiantPirates,

        [EnumMember(Value = "Happosui Army")]
        HapposuiArmy,

        [EnumMember(Value = "AnimalLand of Wano")]
        AnimalLandOfWano,

        [EnumMember(Value = "Monkey Mountain Alliance")]
        MonkeyMountainAlliance,

        [EnumMember(Value = "Scientist")]
        Scientist,

        [EnumMember(Value = "Water SevenNavy")]
        WaterSevenNavy,

        [EnumMember(Value = "Rumbar Pirates")]
        RumbarPirates,

        [EnumMember(Value = "Water Seven")]
        WaterSeven,

        [EnumMember(Value = "FILMCP0")]
        FILMCP0,

        [EnumMember(Value = "Drum KingdomStraw Hat Crew")]
        DrumKingdomStrawHatCrew
    }
}
