using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace ScoreBoardSample
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ScoreBoardSample : UdonSharpBehaviour
    {
        [SerializeField] ScoreBoardGimmick.ScoreBoard _scoreBoard;
        [SerializeField] bool _bestOnly = false;

        float _score = 0;

        public void OnButtonClicked_AddScore_P1()
        {
            OnButtonClicked_AddScore(1);
        }

        public void OnButtonClicked_AddScore_M1()
        {
            OnButtonClicked_AddScore(-1);
        }

        public void OnButtonClicked_AddScore_P0_1()
        {
            OnButtonClicked_AddScore(0.1f);
        }

        public void OnButtonClicked_AddScore_M0_1()
        {
            OnButtonClicked_AddScore(-0.1f);
        }

        public void OnButtonClicked_ClearScore()
        {
            ClearScore();
        }

        public void OnButtonClicked_AddRandomScores()
        {
            var itemRoot = _scoreBoard.transform.Find("#ScoreBoard/#items");
            var itemCount = itemRoot == null ? 3 : itemRoot.childCount;

            for (int i = 0; i < itemCount; i++)
            {
                var playerId = 1000 + i;
                _scoreBoard.UpdateScoreBoard(playerId, $"player {playerId}", i, _bestOnly, true);
            }
        }

        void OnButtonClicked_AddScore(float addScore)
        {
            AddScore(addScore);
        }

        void ClearScore()
        {
            _score = 0;
            _scoreBoard.ClearScoreBoard();
        }

        void AddScore(float addScore)
        {
            var player = Networking.LocalPlayer;
            if (player == null)
            {
                return;
            }

            _score += addScore;
            _scoreBoard.UpdateScoreBoard(player.playerId, player.displayName, _score, _bestOnly, true);
        }
    }
}
