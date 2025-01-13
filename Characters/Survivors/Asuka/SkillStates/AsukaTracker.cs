using RoR2;
using System.Linq;
using UnityEngine;

namespace AsukaMod.Survivors.Asuka.SkillStates
{
    [RequireComponent(typeof(CharacterBody))]
    [RequireComponent(typeof(InputBankTest))]
    [RequireComponent(typeof(TeamComponent))]
    internal class AsukaTracker : MonoBehaviour
    {
        public GameObject trackingPrefab;
        public float maxTrackingAngle = 20f;
        public float updateFreq = 10f;

        private HurtBox trackingTarget;
        private CharacterBody charBody;
        private TeamComponent teamComp;
        private InputBankTest inputBank;
        private float stopWatch;
        private Indicator indicator;
        private readonly BullseyeSearch search = new BullseyeSearch();

        private void Awake()
        {
            if(trackingPrefab == null)
            {
                trackingPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/HuntressTrackingIndicator");
            }
            indicator = new Indicator(gameObject, trackingPrefab);
        }

        private void Start()
        {
            charBody = GetComponent<CharacterBody>();
            inputBank = GetComponent<InputBankTest>();
            teamComp = GetComponent<TeamComponent>();
        }

        public HurtBox GetTrackingTarget()
        {
            return trackingTarget;
        }

        private void FixedUpdate()
        {
            stopWatch += Time.fixedDeltaTime;
            if(stopWatch >= 1 / updateFreq)
            {
                stopWatch -= 1 / updateFreq;
                HurtBox hurtbox = trackingTarget;
                Ray aimRay = new Ray(this.inputBank.aimOrigin, this.inputBank.aimDirection);
                SearchForTarget(aimRay);
                indicator.targetTransform = (this.trackingTarget ? this.trackingTarget.transform : null);
            }
        }

        private void SearchForTarget(Ray aimRay)
        {
            search.teamMaskFilter = TeamMask.all;
            search.teamMaskFilter.RemoveTeam(teamComp.teamIndex);
            search.filterByLoS = true;
            search.searchDirection = aimRay.direction;
            search.searchOrigin = aimRay.origin;
            search.sortMode = BullseyeSearch.SortMode.Angle;
            search.maxAngleFilter = maxTrackingAngle;
            search.RefreshCandidates();
            search.FilterOutGameObject(gameObject);
            trackingTarget = search.GetResults().FirstOrDefault<HurtBox>();
        }
    }
}
