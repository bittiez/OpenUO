#region license

// Copyright (c) 2024, andreakarasho
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by andreakarasho - https://github.com/andreakarasho
// 4. Neither the name of the copyright holder nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System.Collections.Generic;
using System.Linq;
using ClassicUO.Game.Managers;
using ClassicUO.Utility;

namespace ClassicUO.Game.Data
{
    internal static class SpellsMagery
    {
        private static Dictionary<int, SpellDefinition> _spellsDict;
        private static Dictionary<int, List<SpellDefinition>> _spellsCircleDictionary;

        private static string[] _spRegsChars;

        static SpellsMagery()
        {
            _spellsDict = new Dictionary<int, SpellDefinition>
            {
                // first circle
                {
                    1,
                    new SpellDefinition
                    (
                        "Clumsy",
                        1,
                        1,
                        0x1B58,
                        1061290,
                        "Uus Jux",
                        TargetType.Harmful,
                        Reagents.Bloodmoss,
                        Reagents.Nightshade
                    )
                },
                {
                    2,
                    new SpellDefinition
                    (
                        "Create Food",
                        1,
                        2,
                        0x1B59,
                        1061291,
                        "In Mani Ylem",
                        TargetType.Neutral,
                        Reagents.Garlic,
                        Reagents.Ginseng,
                        Reagents.MandrakeRoot
                    )
                },
                {
                    3,
                    new SpellDefinition
                    (
                        "Feeblemind",
                        1,
                        3,
                        0x1B5A,
                        1061292,
                        "Rel Wis",
                        TargetType.Harmful,
                        Reagents.Nightshade,
                        Reagents.Ginseng
                    )
                },
                {
                    4,
                    new SpellDefinition
                    (
                        "Heal",
                        1,
                        4,
                        0x1B5B,
                        1061293,
                        "In Mani",
                        TargetType.Beneficial,
                        Reagents.Garlic,
                        Reagents.Ginseng,
                        Reagents.SpidersSilk
                    )
                },
                {
                    5,
                    new SpellDefinition
                    (
                        "Magic Arrow",
                        1,
                        5,
                        0x1B5C,
                        1061294,
                        "In Por Ylem",
                        TargetType.Harmful,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    6,
                    new SpellDefinition
                    (
                        "Night Sight",
                        1,
                        6,
                        0x1B5D,
                        1061295,
                        "In Lor",
                        TargetType.Beneficial,
                        Reagents.SpidersSilk,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    7,
                    new SpellDefinition
                    (
                        "Reactive Armor",
                        1,
                        7,
                        0x1B5E,
                        1061296,
                        "Flam Sanct",
                        TargetType.Beneficial,
                        Reagents.Garlic,
                        Reagents.SpidersSilk,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    8,
                    new SpellDefinition
                    (
                        "Weaken",
                        1,
                        8,
                        0x1B5F,
                        1061297,
                        "Des Mani",
                        TargetType.Harmful,
                        Reagents.Garlic,
                        Reagents.Nightshade
                    )
                },
                // second circle
                {
                    9,
                    new SpellDefinition
                    (
                        "Agility",
                        2,
                        9,
                        0x1B60,
                        1061298,
                        "Ex Uus",
                        TargetType.Beneficial,
                        Reagents.Bloodmoss,
                        Reagents.MandrakeRoot
                    )
                },
                {
                    10,
                    new SpellDefinition
                    (
                        "Cunning",
                        2,
                        10,
                        0x1B61,
                        1061299,
                        "Uus Wis",
                        TargetType.Beneficial,
                        Reagents.Nightshade,
                        Reagents.MandrakeRoot
                    )
                },
                {
                    11,
                    new SpellDefinition
                    (
                        "Cure",
                        2,
                        11,
                        0x1B62,
                        1061300,
                        "An Nox",
                        TargetType.Beneficial,
                        Reagents.Garlic,
                        Reagents.Ginseng
                    )
                },
                {
                    12,
                    new SpellDefinition
                    (
                        "Harm",
                        2,
                        12,
                        0x1B63,
                        1061301,
                        "An Mani",
                        TargetType.Harmful,
                        Reagents.Nightshade,
                        Reagents.SpidersSilk
                    )
                },
                {
                    13,
                    new SpellDefinition
                    (
                        "Magic Trap",
                        2,
                        13,
                        0x1B64,
                        1061302,
                        "In Jux",
                        TargetType.Neutral,
                        Reagents.Garlic,
                        Reagents.SpidersSilk,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    14,
                    new SpellDefinition
                    (
                        "Magic Untrap",
                        2,
                        14,
                        0x1B65,
                        1061303,
                        "An Jux",
                        TargetType.Neutral,
                        Reagents.Bloodmoss,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    15,
                    new SpellDefinition
                    (
                        "Protection",
                        2,
                        15,
                        0x1B66,
                        1061304,
                        "Uus Sanct",
                        TargetType.Beneficial,
                        Reagents.Garlic,
                        Reagents.Ginseng,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    16,
                    new SpellDefinition
                    (
                        "Strength",
                        2,
                        16,
                        0x1B67,
                        1061305,
                        "Uus Mani",
                        TargetType.Beneficial,
                        Reagents.MandrakeRoot,
                        Reagents.Nightshade
                    )
                },
                // third circle
                {
                    17,
                    new SpellDefinition
                    (
                        "Bless",
                        3,
                        17,
                        0x1B68,
                        1061306,
                        "Rel Sanct",
                        TargetType.Beneficial,
                        Reagents.Garlic,
                        Reagents.MandrakeRoot
                    )
                },
                {
                    18, new SpellDefinition
                    (
                        "Fireball",
                        3,
                        18,
                        0x1B69,
                        1061307,
                        "Vas Flam",
                        TargetType.Harmful,
                        Reagents.BlackPearl
                    )
                },
                {
                    19,
                    new SpellDefinition
                    (
                        "Magic Lock",
                        3,
                        19,
                        0x1B6a,
                        1061308,
                        "An Por",
                        TargetType.Neutral,
                        Reagents.Bloodmoss,
                        Reagents.Garlic,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    20, new SpellDefinition
                    (
                        "Poison",
                        3,
                        20,
                        0x1B6b,
                        1061309,
                        "In Nox",
                        TargetType.Harmful,
                        Reagents.Nightshade
                    )
                },
                {
                    21,
                    new SpellDefinition
                    (
                        "Telekinesis",
                        3,
                        21,
                        0x1B6c,
                        1061310,
                        "Ort Por Ylem",
                        TargetType.Neutral,
                        Reagents.Bloodmoss,
                        Reagents.MandrakeRoot
                    )
                },
                {
                    22,
                    new SpellDefinition
                    (
                        "Teleport",
                        3,
                        22,
                        0x1B6d,
                        1061311,
                        "Rel Por",
                        TargetType.Neutral,
                        Reagents.Bloodmoss,
                        Reagents.MandrakeRoot
                    )
                },
                {
                    23,
                    new SpellDefinition
                    (
                        "Unlock",
                        3,
                        23,
                        0x1B6e,
                        1061312,
                        "Ex Por",
                        TargetType.Neutral,
                        Reagents.Bloodmoss,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    24,
                    new SpellDefinition
                    (
                        "Wall of Stone",
                        3,
                        24,
                        0x1B6f,
                        1061313,
                        "In Sanct Ylem",
                        TargetType.Neutral,
                        Reagents.Bloodmoss,
                        Reagents.Garlic
                    )
                },
                // fourth circle
                {
                    25,
                    new SpellDefinition
                    (
                        "Arch Cure",
                        4,
                        25,
                        0x1B70,
                        1061314,
                        "Vas An Nox",
                        TargetType.Beneficial,
                        Reagents.Garlic,
                        Reagents.Ginseng,
                        Reagents.MandrakeRoot
                    )
                },
                {
                    26,
                    new SpellDefinition
                    (
                        "Arch Protection",
                        4,
                        26,
                        0x1B71,
                        1061315,
                        "Vas Uus Sanct",
                        TargetType.Beneficial,
                        Reagents.Garlic,
                        Reagents.Ginseng,
                        Reagents.MandrakeRoot,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    27,
                    new SpellDefinition
                    (
                        "Curse",
                        4,
                        27,
                        0x1B72,
                        1061316,
                        "Des Sanct",
                        TargetType.Harmful,
                        Reagents.Garlic,
                        Reagents.Nightshade,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    28,
                    new SpellDefinition
                    (
                        "Fire Field",
                        4,
                        28,
                        0x1B73,
                        1061317,
                        "In Flam Grav",
                        TargetType.Neutral,
                        Reagents.BlackPearl,
                        Reagents.SpidersSilk,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    29,
                    new SpellDefinition
                    (
                        "Greater Heal",
                        4,
                        29,
                        0x1B74,
                        1061318,
                        "In Vas Mani",
                        TargetType.Beneficial,
                        Reagents.Garlic,
                        Reagents.Ginseng,
                        Reagents.MandrakeRoot,
                        Reagents.SpidersSilk
                    )
                },
                {
                    30,
                    new SpellDefinition
                    (
                        "Lightning",
                        4,
                        30,
                        0x1B75,
                        1061319,
                        "Por Ort Grav",
                        TargetType.Harmful,
                        Reagents.MandrakeRoot,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    31,
                    new SpellDefinition
                    (
                        "Mana Drain",
                        4,
                        31,
                        0x1B76,
                        1061320,
                        "Ort Rel",
                        TargetType.Harmful,
                        Reagents.BlackPearl,
                        Reagents.MandrakeRoot,
                        Reagents.SpidersSilk
                    )
                },
                {
                    32,
                    new SpellDefinition
                    (
                        "Recall",
                        4,
                        32,
                        0x1B77,
                        1061321,
                        "Kal Ort Por",
                        TargetType.Neutral,
                        Reagents.BlackPearl,
                        Reagents.Bloodmoss,
                        Reagents.MandrakeRoot
                    )
                },
                // fifth circle
                {
                    33,
                    new SpellDefinition
                    (
                        "Blade Spirits",
                        5,
                        33,
                        0x1B78,
                        1061322,
                        "In Jux Hur Ylem",
                        TargetType.Neutral,
                        Reagents.BlackPearl,
                        Reagents.MandrakeRoot,
                        Reagents.Nightshade
                    )
                },
                {
                    34,
                    new SpellDefinition
                    (
                        "Dispel Field",
                        5,
                        34,
                        0x1B79,
                        1061323,
                        "An Grav",
                        TargetType.Neutral,
                        Reagents.BlackPearl,
                        Reagents.Garlic,
                        Reagents.SpidersSilk,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    35,
                    new SpellDefinition
                    (
                        "Incognito",
                        5,
                        35,
                        0x1B7a,
                        1061324,
                        "Kal In Ex",
                        TargetType.Neutral,
                        Reagents.Bloodmoss,
                        Reagents.Garlic,
                        Reagents.Nightshade
                    )
                },
                {
                    36,
                    new SpellDefinition
                    (
                        "Magic Reflection",
                        5,
                        36,
                        0x1B7b,
                        1061325,
                        "In Jux Sanct",
                        TargetType.Beneficial,
                        Reagents.Garlic,
                        Reagents.MandrakeRoot,
                        Reagents.SpidersSilk
                    )
                },
                {
                    37,
                    new SpellDefinition
                    (
                        "Mind Blast",
                        5,
                        37,
                        0x1B7c,
                        1061326,
                        "Por Corp Wis",
                        TargetType.Harmful,
                        Reagents.BlackPearl,
                        Reagents.MandrakeRoot,
                        Reagents.Nightshade,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    38,
                    new SpellDefinition
                    (
                        "Paralyze",
                        5,
                        38,
                        0x1B7d,
                        1061327,
                        "An Ex Por",
                        TargetType.Harmful,
                        Reagents.Garlic,
                        Reagents.MandrakeRoot,
                        Reagents.SpidersSilk
                    )
                },
                {
                    39,
                    new SpellDefinition
                    (
                        "Poison Field",
                        5,
                        39,
                        0x1B7e,
                        1061328,
                        "In Nox Grav",
                        TargetType.Neutral,
                        Reagents.BlackPearl,
                        Reagents.Nightshade,
                        Reagents.SpidersSilk
                    )
                },
                {
                    40,
                    new SpellDefinition
                    (
                        "Summon Creature",
                        5,
                        40,
                        0x1B7f,
                        1061329,
                        "Kal Xen",
                        TargetType.Neutral,
                        Reagents.Bloodmoss,
                        Reagents.MandrakeRoot,
                        Reagents.SpidersSilk
                    )
                },
                // sixth circle
                {
                    41,
                    new SpellDefinition
                    (
                        "Dispel",
                        6,
                        41,
                        0x1B80,
                        1061330,
                        "An Ort",
                        TargetType.Neutral,
                        Reagents.Garlic,
                        Reagents.MandrakeRoot,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    42,
                    new SpellDefinition
                    (
                        "Energy Bolt",
                        6,
                        42,
                        0x1B81,
                        1061331,
                        "Corp Por",
                        TargetType.Harmful,
                        Reagents.BlackPearl,
                        Reagents.Nightshade
                    )
                },
                {
                    43,
                    new SpellDefinition
                    (
                        "Explosion",
                        6,
                        43,
                        0x1B82,
                        1061332,
                        "Vas Ort Flam",
                        TargetType.Harmful,
                        Reagents.Bloodmoss,
                        Reagents.MandrakeRoot
                    )
                },
                {
                    44,
                    new SpellDefinition
                    (
                        "Invisibility",
                        6,
                        44,
                        0x1B83,
                        1061333,
                        "An Lor Xen",
                        TargetType.Beneficial,
                        Reagents.Bloodmoss,
                        Reagents.Nightshade
                    )
                },
                {
                    45,
                    new SpellDefinition
                    (
                        "Mark",
                        6,
                        45,
                        0x1B84,
                        1061334,
                        "Kal Por Ylem",
                        TargetType.Neutral,
                        Reagents.BlackPearl,
                        Reagents.Bloodmoss,
                        Reagents.MandrakeRoot
                    )
                },
                {
                    46,
                    new SpellDefinition
                    (
                        "Mass Curse",
                        6,
                        46,
                        0x1B85,
                        1061335,
                        "Vas Des Sanct",
                        TargetType.Harmful,
                        Reagents.Garlic,
                        Reagents.MandrakeRoot,
                        Reagents.Nightshade,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    47,
                    new SpellDefinition
                    (
                        "Paralyze Field",
                        6,
                        47,
                        0x1B86,
                        1061336,
                        "In Ex Grav",
                        TargetType.Neutral,
                        Reagents.BlackPearl,
                        Reagents.Ginseng,
                        Reagents.SpidersSilk
                    )
                },
                {
                    48,
                    new SpellDefinition
                    (
                        "Reveal",
                        6,
                        48,
                        0x1B87,
                        1061337,
                        "Wis Quas",
                        TargetType.Neutral,
                        Reagents.Bloodmoss,
                        Reagents.SulfurousAsh
                    )
                },
                // seventh circle
                {
                    49,
                    new SpellDefinition
                    (
                        "Chain Lightning",
                        7,
                        49,
                        0x1B88,
                        1061338,
                        "Vas Ort Grav",
                        TargetType.Harmful,
                        Reagents.BlackPearl,
                        Reagents.Bloodmoss,
                        Reagents.MandrakeRoot,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    50,
                    new SpellDefinition
                    (
                        "Energy Field",
                        7,
                        50,
                        0x1B89,
                        1061339,
                        "In Sanct Grav",
                        TargetType.Neutral,
                        Reagents.BlackPearl,
                        Reagents.MandrakeRoot,
                        Reagents.SpidersSilk,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    51,
                    new SpellDefinition
                    (
                        "Flamestrike",
                        7,
                        51,
                        0x1B8a,
                        1061340,
                        "Kal Vas Flam",
                        TargetType.Harmful,
                        Reagents.SpidersSilk,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    52,
                    new SpellDefinition
                    (
                        "Gate Travel",
                        7,
                        52,
                        0x1B8b,
                        1061341,
                        "Vas Rel Por",
                        TargetType.Neutral,
                        Reagents.BlackPearl,
                        Reagents.MandrakeRoot,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    53,
                    new SpellDefinition
                    (
                        "Mana Vampire",
                        7,
                        53,
                        0x1B8c,
                        1061342,
                        "Ort Sanct",
                        TargetType.Harmful,
                        Reagents.BlackPearl,
                        Reagents.Bloodmoss,
                        Reagents.MandrakeRoot,
                        Reagents.SpidersSilk
                    )
                },
                {
                    54,
                    new SpellDefinition
                    (
                        "Mass Dispel",
                        7,
                        54,
                        0x1B8d,
                        1061343,
                        "Vas An Ort",
                        TargetType.Neutral,
                        Reagents.BlackPearl,
                        Reagents.Garlic,
                        Reagents.MandrakeRoot,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    55,
                    new SpellDefinition
                    (
                        "Meteor Swarm",
                        7,
                        55,
                        0x1B8e,
                        1061344,
                        "Flam Kal Des Ylem",
                        TargetType.Harmful,
                        Reagents.Bloodmoss,
                        Reagents.MandrakeRoot,
                        Reagents.SpidersSilk,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    56,
                    new SpellDefinition
                    (
                        "Polymorph",
                        7,
                        56,
                        0x1B8f,
                        1061345,
                        "Vas Ylem Rel",
                        TargetType.Neutral,
                        Reagents.Bloodmoss,
                        Reagents.MandrakeRoot,
                        Reagents.SpidersSilk
                    )
                },
                // eighth circle
                {
                    57,
                    new SpellDefinition
                    (
                        "Earthquake",
                        8,
                        57,
                        0x1B90,
                        1061346,
                        "In Vas Por",
                        TargetType.Harmful,
                        Reagents.Bloodmoss,
                        Reagents.Ginseng,
                        Reagents.MandrakeRoot,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    58,
                    new SpellDefinition
                    (
                        "Energy Vortex",
                        8,
                        58,
                        0x1B91,
                        1061347,
                        "Vas Corp Por",
                        TargetType.Neutral,
                        Reagents.BlackPearl,
                        Reagents.Bloodmoss,
                        Reagents.MandrakeRoot,
                        Reagents.Nightshade
                    )
                },
                {
                    59,
                    new SpellDefinition
                    (
                        "Resurrection",
                        8,
                        59,
                        0x1B92,
                        1061348,
                        "An Corp",
                        TargetType.Beneficial,
                        Reagents.Bloodmoss,
                        Reagents.Ginseng,
                        Reagents.Garlic
                    )
                },
                {
                    60,
                    new SpellDefinition
                    (
                        "Air Elemental",
                        8,
                        60,
                        0x1B93,
                        1061349,
                        "Kal Vas Xen Hur",
                        TargetType.Neutral,
                        Reagents.Bloodmoss,
                        Reagents.MandrakeRoot,
                        Reagents.SpidersSilk
                    )
                },
                {
                    61,
                    new SpellDefinition
                    (
                        "Summon Daemon",
                        8,
                        61,
                        0x1B94,
                        1061350,
                        "Kal Vas Xen Corp",
                        TargetType.Neutral,
                        Reagents.Bloodmoss,
                        Reagents.MandrakeRoot,
                        Reagents.SpidersSilk,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    62,
                    new SpellDefinition
                    (
                        "Earth Elemental",
                        8,
                        62,
                        0x1B95,
                        1061351,
                        "Kal Vas Xen Ylem",
                        TargetType.Neutral,
                        Reagents.Bloodmoss,
                        Reagents.MandrakeRoot,
                        Reagents.SpidersSilk
                    )
                },
                {
                    63,
                    new SpellDefinition
                    (
                        "Fire Elemental",
                        8,
                        63,
                        0x1B96,
                        1061352,
                        "Kal Vas Xen Flam",
                        TargetType.Neutral,
                        Reagents.Bloodmoss,
                        Reagents.MandrakeRoot,
                        Reagents.SpidersSilk,
                        Reagents.SulfurousAsh
                    )
                },
                {
                    64,
                    new SpellDefinition
                    (
                        "Water Elemental",
                        8,
                        64,
                        0x1B97,
                        1061353,
                        "Kal Vas Xen An Flam",
                        TargetType.Neutral,
                        Reagents.Bloodmoss,
                        Reagents.MandrakeRoot,
                        Reagents.SpidersSilk
                    )
                }
            };

            _spellsCircleDictionary = new Dictionary<int, List<SpellDefinition>>();
            RebuildCircleDict();
        }

        public static string SpellBookName { get; set; } = SpellBookType.Magery.ToString();

        public static Dictionary<int, SpellDefinition> GetAllSpells => _spellsDict;
        
        public static Dictionary<int, List<SpellDefinition>> GetAllCircles => _spellsCircleDictionary;
        internal static int MaxSpellCount => _spellsDict.Count;

        public static string[] CircleNames { get; } =
        {
            "First Circle", "Second Circle", "Third Circle", "Fourth Circle", "Fifth Circle", "Sixth Circle",
            "Seventh Circle", "Eighth Circle"
        };

        public static string[] SpecialReagentsChars
        {
            get
            {
                if (_spRegsChars == null)
                {
                    _spRegsChars = new string[_spellsDict.Max(o => o.Key)];

                    for (int i = _spRegsChars.Length; i > 0; --i)
                    {
                        if (_spellsDict.TryGetValue(i, out SpellDefinition sd))
                        {
                            _spRegsChars[i - 1] = StringHelper.RemoveUpperLowerChars(sd.PowerWords);
                        }
                        else
                        {
                            _spRegsChars[i - 1] = string.Empty;
                        }
                    }
                }

                return _spRegsChars;
            }
        }

        public static SpellDefinition GetSpell(int index)
        {
            return _spellsDict.TryGetValue(index, out SpellDefinition spell) ? spell : SpellDefinition.EmptySpell;
        }

        public static void SetSpell(int id, in SpellDefinition newspell)
        {
            _spRegsChars = null;
            _spellsDict[id] = newspell;
            RebuildCircleDict();
        }

        internal static void Clear()
        {
            _spellsDict.Clear();
        }

        internal static void RebuildCircleDict()
        {
            _spellsCircleDictionary.Clear();

            foreach (var spell in _spellsDict.Values)
            {
                if (!_spellsCircleDictionary.ContainsKey(spell.SpellCircle))
                    _spellsCircleDictionary.Add(spell.SpellCircle, new List<SpellDefinition>());
                _spellsCircleDictionary[spell.SpellCircle].Add(spell);
            }
        }
    }
}