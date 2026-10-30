using System.Collections.Generic;
using UnityEngine;

namespace Tractor
{
    public class SeasonsMaterialsController : MonoBehaviour
    {
        public static SeasonsMaterialsController Instance { get; private set; }

        public List<SeasonsMaterials> seasonsMaterials = new List<SeasonsMaterials>();
        SeasonsMaterials.Season? currentSeason;

        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create SeasonsMaterialsController");
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Register(SeasonsMaterials item)
        {
            if (item == null || seasonsMaterials.Contains(item)) return;
            seasonsMaterials.Add(item);

            if (currentSeason.HasValue) item.ChangeSeason(currentSeason.Value);
        }

        public void Unregister(SeasonsMaterials item)
        {
            if (item == null) return;
            seasonsMaterials.Remove(item);
        }

        public void ChangeSeason(SeasonsMaterials.Season season)
        {
            currentSeason = season;

            for (int i = 0; i < seasonsMaterials.Count; i++)
            {
                if (seasonsMaterials[i] == null) seasonsMaterials.RemoveAt(i);
                else seasonsMaterials[i].ChangeSeason(season);
            }
        }
    }
}
