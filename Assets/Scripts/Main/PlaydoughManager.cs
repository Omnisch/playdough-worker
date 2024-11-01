using System.Collections;
using UnityEngine;

namespace Omnis.Playdough
{
    public partial class PlaydoughManager : InteractBase
    {
        #region Serialized Fields
        [SerializeField] private GameObject playdoughPrefab;
        [Header("Input")]
        [SerializeField] private GameObject crosshair;
        [SerializeField] private float sensitivity;
        [SerializeField] private Vector3 slideInPosition;
        [SerializeField] private Vector3 slideOutPosition;
        #endregion

        #region Fields
        private Playdough playdough;
        private Vector2 startPointerPosition;
        private float startAspectRatio;
        private float startTime;
        #endregion

        #region Interfaces
        public override bool IsLeftPressed
        {
            get => base.IsLeftPressed;
            set
            {
                base.IsLeftPressed = value;
                if (value)
                {
                    startPointerPosition = InputHandler.PointerPosition;
                    startTime = Time.realtimeSinceStartup;
                    if (playdough) startAspectRatio = playdough.AspectRatio;
                }
                else
                {
                    if (playdough) SettleScore(playdough.AspectRatio);
                    SpawnPlaydough();
                }
            }
        }

        public void BackToStartScene() => UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("StartScene");
        #endregion

        #region Functions
        private void SpawnPlaydough()
        {
            if (playdough)
                playdough.SlideOutAndDestroy(slideOutPosition);

            playdough = Instantiate(playdoughPrefab).GetComponent<Playdough>();
            playdough.Shape = GameSettings.GetRandomShapeFromPool();
            if (GameSettings.RandomScale)
                playdough.Scale = Random.Range(1f, 2f);
            else
                playdough.Scale = 1.5f;
            if (GameSettings.RandomRotation)
                playdough.Rotation = Random.Range(0f, 360f);
            playdough.AspectRatio = Mathf.Sign(Random.Range(-1f, 1f)) * Random.Range(0.25f, 0.5f);
            playdough.SlideIn(slideInPosition);
        }

        private Playdough SpawnPerfectPhantom(Color color)
        {
            if (!GameSettings.EnablePhantoms) return null;
            if (!playdough) return null;

            var phantom = Instantiate(playdoughPrefab).GetComponent<Playdough>();
            playdough.CopyTo(phantom);
            phantom.AspectRatio = 0f;
            phantom.Color = color;
            phantom.gameObject.AddComponent<TTLMonoBehaviour>().SetLifeTime(0.5f).OnLifeSpan = (value) => phantom.Color = new(phantom.Color.r, phantom.Color.g, phantom.Color.b, value);
            return phantom;
        }

        private void GameOver()
        {
            Interactable = false;
            Statistics.AddOneRoundToTotal();
            statsPanel.ShowPanel();
        }
        #endregion

        #region Unity Methods
        protected override void Start()
        {
            base.Start();
            Statistics.ResetOneRoundStatistics();
            crosshair.SetActive(GameSettings.EnableCrosshair);
            SetQualifiedScore();
            Countdown = 10f;
            SpawnPlaydough();
        }

        private void Update()
        {
            if (!Interactable) return;

            Countdown -= Time.deltaTime;
            Statistics.playedTime += Time.deltaTime;
            if (Countdown <= 0f) GameOver();

            if (IsLeftPressed)
            {
                playdough.AspectRatio = startAspectRatio
                    + Mathf.Sign(startPointerPosition.x - Screen.width / 2) * (0.001f * sensitivity) * (InputHandler.PointerPosition.x - startPointerPosition.x)
                    + Mathf.Sign(Screen.height / 2 - startPointerPosition.y) * (0.001f * sensitivity) * (InputHandler.PointerPosition.y - startPointerPosition.y);
            }
        }
        #endregion
    }
}
