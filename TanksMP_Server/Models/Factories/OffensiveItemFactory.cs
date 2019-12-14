﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TanksMP_Server.Models.ItemModels;

namespace TanksMP_Server.Models.Factories
{
    public class OffensiveItemFactory : ItemFactory
    {
        public override IConsumablePowerUp createConsumablePowerUp()
        {
            return new Mine();
        }

        public override IStatusPowerUp createStatusPowerUp()
        {
            return new DamagePowerUp();
        }
    }
}
