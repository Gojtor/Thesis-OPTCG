﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCGSim
{
    public enum GameState
    {
       MAINMENU,
       INGAMEMENU,
       CONNECTING,
       WAITINGFOROPPONENT,
       STARTINGPHASE,
       PLAYERPHASE,
       ENEMYPHASE,
       MATCHLOST,
       MATCHWON
    }
}
