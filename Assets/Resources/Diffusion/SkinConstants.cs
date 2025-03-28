using System.Collections.Generic;

public static class SkinConstants
{
        public enum RequestMode
    {
        None,
        SelectSkin,
        GenerateSkin
    }

        public enum SkinPart
    {
        Head,
        Torso,
        Both
    }


    public static readonly Dictionary<string, string> skinDescriptionToSkinFile = new Dictionary<string, string> {
        {"worker green male smiling brown-hair", "fantasyMaleB.png"},
        {"blacksmith male grin brown-hair", "fantasyMaleA.png"},
        {"military female green angry brown-hair", "militaryFemaleA.png"},
        {"military female brown smiling blonde-hair", "militaryFemaleB.png"},
        {"racer purple female smiling brown-hair", "racerPurpleFemale.png"},
        {"alien light-blue happy", "alienC.png"},
        {"alien green-yellow scared", "alienB.png"},
        {"alien teal neutral-mood", "alienA.png"},
        {"astronaut male orange-spacesuit brown-hair ", "astroMaleB.png"},
        {"worker green female", "fantasyFemaleB.png"},
        {"cyborg female half-alien", "cyborgFemaleA.png"},
        {"cyborg male half-alien ", "cyborg.png"},
        {"farmer blue-white", "farmerA.png"},
        {"astronaut male grey happy brown-hair", "astroMaleA.png"},
        {"blacksmith female ginger hair", "fantasyFemaleA.png"},
        {"athlete male red", "athleteMaleRed.png"},
        {"racer blue male", "racerBlueMale.png"},
        {"athlete female yellow", "athleteFemaleYellow.png"},
        {"racer orange male", "racerOrangeMale.png"},
        {"farmer grey-red", "farmerB.png"},
        {"athlete female blue", "athleteFemaleBlue.png"},
        {"robot pink eyes", "robot2.png"},
        {"robot green eyes triangle", "robot3.png"},
        {"racer red female", "racerRedFemale.png"},
        {"racer orange female", "racerOrangeFemale.png"},
        {"racer red male", "racerRedMale.png"},
        {"athlete male green", "athleteMaleGreen.png"},
        {"criminal male moustache", "criminalMaleA.png"},
        {"athlete male yellow", "athleteMaleYellow.png"},
        {"racer purple male", "racerPurpleMale.png"},
        {"robot yellow eyes", "robot.png"},
        {"military male green", "militaryMaleB.png"},
        {"athlete Female Red", "athleteFemaleRed.png"},
        {"military male brown", "militaryMaleA.png"},
        {"female yellow shirt", "casualFemaleA.png"},
        {"zombie angry", "zombieA.png"},
        {"male white shirt blue tie", "businessMaleA.png"},
        {"female fringe alternative", "skaterFemaleA.png"},
        {"female ginger-hair blue t-shirt smile green", "casualFemaleB.png"},
        {"male eyebrow piercing goatee smile skull red vest", "skaterMaleA.png"},
        {"zombie hair clothes grey brown-hair evil danger grumpy evil", "zombieB.png"},
        {"male black-suit white-shirt red-tie tailor business man money", "businessMaleB.png"},
        {"female racer green muddy ginger-hair", "racerGreenFemale.png"},
        {"athlete Male Blue runner sporty brown vest", "athleteMaleBlue.png"},
        {"zombie skull blue grey tshirt dark-brown dangerous", "zombieC.png"},
        {"athlete Female Green smile ginger t-shirt running sport", "athleteFemaleGreen.png"},
        {"astronaut female grey brown danger smile brave", "astroFemaleA.png"},
        {"white blue t-shirt muddy dirty chestnut grumpy", "survivorMaleB.png"},
        {"racer green rally brown muddy dirt smiling happy", "racerGreenMale.png"},
        {"muddy brown blue t-shirt danger outside grin", "survivorFemaleA.png"},
        {"racer speed blue slick oil brown-hair smile", "racerBlueFemale.png"},
        {"Blue casual relaxed t-shirt grin", "casualMaleB.png"},
        {"astronaut female orange brave grin", "astroFemaleB.png"},
        {"strong dirty muddy blue belt danger outside", "survivorMaleA.png"},
        {"survivor black dirty muddy female strong", "survivorFemaleB.png"},
        {"man male pale t-shirt", "casualMaleA.png"}
    };

    public static readonly Dictionary<string, int> skinFileToID = new Dictionary<string, int> {
        {"alienA.png", 0},
        {"alienB.png", 1},
        {"alienC.png", 2},
        {"astroFemaleA.png", 3},
        {"astroFemaleB.png", 4},
        {"astroMaleA.png", 5},
        {"astroMaleB.png", 6},
        {"athleteFemaleBlue.png", 7},
        {"athleteFemaleGreen.png", 8},
        {"athleteFemaleRed.png", 9},
        {"athleteFemaleYellow.png", 10},
        {"athleteMaleBlue.png", 11},
        {"athleteMaleGreen.png", 12},
        {"athleteMaleRed.png", 13},
        {"athleteMaleYellow.png", 14},
        {"businessMaleA.png", 15},
        {"businessMaleB.png", 16},
        {"casualFemaleA.png", 17},
        {"casualFemaleB.png", 18},
        {"casualMaleA.png", 19},
        {"casualMaleB.png", 20},
        {"criminalMaleA.png", 21},
        {"cyborg.png", 22},
        {"cyborgFemaleA.png", 23},
        {"fantasyFemaleA.png", 24},
        {"fantasyFemaleB.png", 25},
        {"fantasyMaleA.png", 26},
        {"fantasyMaleB.png", 27},
        {"farmerA.png", 28},
        {"farmerB.png", 29},
        {"militaryFemaleA.png", 30},
        {"militaryFemaleB.png", 31},
        {"militaryMaleA.png", 32},
        {"militaryMaleB.png", 33},
        {"racerBlueFemale.png", 34},
        {"racerBlueMale.png", 35},
        {"racerGreenFemale.png", 36},
        {"racerGreenMale.png", 37},
        {"racerOrangeFemale.png", 38},
        {"racerOrangeMale.png", 39},
        {"racerPurpleFemale.png", 40},
        {"racerPurpleMale.png", 41},
        {"racerRedFemale.png", 42},
        {"racerRedMale.png", 43},
        {"robot.png", 44},
        {"robot2.png", 45},
        {"robot3.png", 46},
        {"skaterFemaleA.png", 47},
        {"skaterMaleA.png", 48},
        {"survivorFemaleA.png", 49},
        {"survivorFemaleB.png", 50},
        {"survivorMaleA.png", 51},
        {"survivorMaleB.png", 52},
        {"zombieA.png", 53},
        {"zombieB.png", 54},
        {"zombieC.png", 55}
    };
}
