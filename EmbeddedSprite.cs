﻿using System;
using UnityEngine;
using ItemChanger;
using ItemChanger.Internal;

namespace VendorRando {
    //yes I stole this from RandoPlus
    [Serializable]
    public class EmbeddedSprite: ISprite {
        private static SpriteManager EmbeddedSpriteManager = new(typeof(EmbeddedSprite).Assembly, "VendorRando.Resources.Pins.");

        public string key;
        public EmbeddedSprite(string key) {
            this.key = key;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Sprite Value => EmbeddedSpriteManager.GetSprite(key);
        public ISprite Clone() => (ISprite)MemberwiseClone();
    }
}