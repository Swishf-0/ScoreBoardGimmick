// #define DEBUG_LOG
// #define DEBUG_SCORE_BOARD

using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace ScoreBoardGimmick
{
    enum ScoreBoardMessageType
    {
        REQUEST_SYNC_SCORE_BOARD,
        SEND_SCORE_BOARD,
        SEND_SINGLE_SCORE,
        SEND_SINGLE_SCORE_BEST_ONLY,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ScoreBoard : UdonSharpBehaviour
    {
        [SerializeField] bool _smallerIsBetter;
        [SerializeField] bool _isFloat;
        [SerializeField, Range(1, 10)] int _numberOfFloatDigits = 1;

        bool _initialized;

        void Start()
        {
            Initialize();
        }

        void Update()
        {
            Update_();
        }

        void Initialize()
        {
            InitScoreBoard();
            SendCustomEventDelayedSeconds(nameof(OnInitialized), 3);
        }

        public void OnInitialized()
        {
            _initialized = true;

            if (!Utils.SafeGetIsMaster())
            {
                SendNetwork_RequestSyncScoreBoard();
            }
        }

        void Update_()
        {
#if DEBUG_SCORE_BOARD
            Test_ScoreBoardCommandCheck();
#endif

            if (!_initialized)
            {
                return;
            }
        }

        public override void OnPostSerialization(VRC.Udon.Common.SerializationResult result)
        {
            if (result.success)
            {
                _Debug.Log($"OnPostSerialization ByteCount: {result.byteCount}");
            }
        }








#if DEBUG_SCORE_BOARD
        [SerializeField] int _playerId = 0;
        [SerializeField] string _playerName = "a0";
        [SerializeField] float _score = 1.23f;
        [SerializeField] bool _bestOnly = true;

        void Test_ScoreBoardCommandCheck()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                UpdateScoreBoard(_playerId, _playerName, _score, _bestOnly, true);
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                UpdateScoreBoard(0, "a0", 1.230f, true, false);
                UpdateScoreBoard(4, "a4", 1.234f, true, false);
                UpdateScoreBoard(6, "a6", 1.236f, true, false);
                UpdateScoreBoard(5, "a5", 1.235f, true, false);
                UpdateScoreBoard(3, "a3", 1.233f, true, false);
                UpdateScoreBoard(2, "a2", 1.232f, true, false);
                UpdateScoreBoard(1, "a1", 1.231f, true, false);
                UpdateScoreBoard(7, "a7", 1.237f, true, false);
                UpdateScoreBoard(0, "a08", 1.238f, true, false);
                UpdateScoreBoard(0, "a09", 1.229f, true, false);
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                UpdateScoreBoard(Networking.LocalPlayer.playerId, Networking.LocalPlayer.displayName, _score - Networking.LocalPlayer.playerId, _bestOnly, true);
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                UpdateScoreBoard(Networking.LocalPlayer.playerId, Networking.LocalPlayer.displayName, _score + Networking.LocalPlayer.playerId, _bestOnly, true);
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                _bestOnly = !_bestOnly;
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                SendNetwork_RequestSyncScoreBoard();
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                SendNetwork_SendScoreBoard();
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                ClearScoreBoard();
            }
        }
#endif








        public void UpdateScoreBoard(int playerId, string playerName, float score, bool bestOnly, bool sync)
        {
            if (!UpdateScore(playerId, playerName, score, bestOnly))
            {
                return;
            }
            ApplyScoresToUI();

            if (sync)
            {
                SendNetwork_SendSingleScore(playerId, score, bestOnly);
            }
        }

        public void ClearScoreBoard()
        {
            ClearScoreBoardData();
            ApplyScoresToUI();
        }

        void ApplyScoresToUI()
        {
            for (int i = 0; i < _scoreBoardItemCount; i++)
            {
                var hasScore = HasScore(_scoreBoardPlayerIds[i]);
                if (hasScore)
                {
                    _scoreBoardRankTexts[i].text = $"{i + 1}";
                    _scoreBoardNameTexts[i].text = $"{_scoreBoardPlayerNames[i]}";
                    _scoreBoardPointTexts[i].text = ScoreToUIText(_scoreBoardScores[i]);
                }
                else
                {
                    _scoreBoardRankTexts[i].text = string.Empty;
                    _scoreBoardNameTexts[i].text = string.Empty;
                    _scoreBoardPointTexts[i].text = string.Empty;
                }

                var parentObj = _scoreBoardRankTexts[i].transform.parent.gameObject;
                if (hasScore != parentObj.activeSelf)
                {
                    parentObj.SetActive(hasScore);
                }
            }
        }

        string ScoreToUIText(float score)
        {
            return _isFloat ? $"{score.ToString($"F{_numberOfFloatDigits}")}" : $"{Mathf.RoundToInt(score)}";
        }








        TextMeshProUGUI[] _scoreBoardRankTexts, _scoreBoardNameTexts, _scoreBoardPointTexts;

        void InitScoreBoard()
        {
            InitScoreBoardUI();
            InitializeScoreBoardData();
        }

        void InitScoreBoardUI()
        {
            var itemRoot = transform.Find("#ScoreBoard/#items");
            _scoreBoardItemCount = itemRoot.childCount;
            _scoreBoardRankTexts = new TextMeshProUGUI[_scoreBoardItemCount];
            _scoreBoardNameTexts = new TextMeshProUGUI[_scoreBoardItemCount];
            _scoreBoardPointTexts = new TextMeshProUGUI[_scoreBoardItemCount];

            for (int i = 0; i < _scoreBoardItemCount; i++)
            {
                var t = itemRoot.GetChild(i);

                _scoreBoardRankTexts[i] = t.Find("#rank").GetComponent<TextMeshProUGUI>();
                _scoreBoardNameTexts[i] = t.Find("#name").GetComponent<TextMeshProUGUI>();
                _scoreBoardPointTexts[i] = t.Find("#score").GetComponent<TextMeshProUGUI>();
            }

            ClearScoreBoardUI();
        }

        void ClearScoreBoardUI()
        {
            for (int i = 0; i < _scoreBoardRankTexts.Length; i++)
            {
                _scoreBoardRankTexts[i].text = string.Empty;
                _scoreBoardNameTexts[i].text = string.Empty;
                _scoreBoardPointTexts[i].text = string.Empty;

                var parentObj = _scoreBoardPointTexts[i].transform.parent.gameObject;
                if (parentObj.activeSelf)
                {
                    parentObj.SetActive(false);
                }
            }
        }








        const int INVALID_PLAYER_ID = -1;
        int _scoreBoardItemCount;
        int[] _scoreBoardPlayerIds;
        string[] _scoreBoardPlayerNames;
        float[] _scoreBoardScores;

        void InitializeScoreBoardData()
        {
            _scoreBoardPlayerIds = new int[_scoreBoardItemCount];
            _scoreBoardPlayerNames = new string[_scoreBoardItemCount];
            _scoreBoardScores = new float[_scoreBoardItemCount];

            ClearScoreBoardData();
        }

        void ClearScoreBoardData()
        {
            for (int i = 0; i < _scoreBoardItemCount; i++)
            {
                _scoreBoardPlayerIds[i] = INVALID_PLAYER_ID;
                _scoreBoardPlayerNames[i] = string.Empty;
                _scoreBoardScores[i] = 0;
            }
        }

        bool IsBetterScore(float origScore, float newScore)
        {
            return _smallerIsBetter ? origScore > newScore : origScore < newScore;
        }

        bool HasScore(int playerId)
        {
            return playerId != INVALID_PLAYER_ID;
        }

        bool UpdateScore(int playerId, string playerName, float score, bool bestOnly)
        {
            if (!HasScore(playerId))
            {
                return false;
            }

            bool hasScoreOnBoard = false;
            bool needMoveDown = false;
            int scoreIdx = _scoreBoardItemCount - 1;
            for (int i = 0; i < _scoreBoardItemCount; i++)
            {
                if (_scoreBoardPlayerIds[i] == playerId && _scoreBoardPlayerNames[i] == playerName)
                {
                    if (bestOnly && !IsBetterScore(_scoreBoardScores[i], score))
                    {
                        return false;
                    }

                    hasScoreOnBoard = true;
                    scoreIdx = i;
                    needMoveDown = !bestOnly && (i != _scoreBoardItemCount - 1) && HasScore(_scoreBoardPlayerIds[i + 1]) && IsBetterScore(score, _scoreBoardScores[i + 1]);
                    _scoreBoardScores[i] = score;

                    break;
                }
            }
            if (!hasScoreOnBoard)
            {
                if (HasScore(_scoreBoardPlayerIds[_scoreBoardItemCount - 1]) && !IsBetterScore(_scoreBoardScores[_scoreBoardItemCount - 1], score))
                {
                    return false;
                }

                _scoreBoardPlayerIds[_scoreBoardItemCount - 1] = playerId;
                _scoreBoardPlayerNames[_scoreBoardItemCount - 1] = playerName;
                _scoreBoardScores[_scoreBoardItemCount - 1] = score;
            }

            if (needMoveDown)
            {
                for (int i = scoreIdx; i < _scoreBoardItemCount; i++)
                {
                    if (i == _scoreBoardItemCount - 1
                        || !HasScore(_scoreBoardPlayerIds[i + 1])
                        || !IsBetterScore(score, _scoreBoardScores[i + 1]))
                    {
                        _scoreBoardPlayerIds[i] = playerId;
                        _scoreBoardPlayerNames[i] = playerName;
                        _scoreBoardScores[i] = score;

                        break;
                    }
                    _scoreBoardPlayerIds[i] = _scoreBoardPlayerIds[i + 1];
                    _scoreBoardPlayerNames[i] = _scoreBoardPlayerNames[i + 1];
                    _scoreBoardScores[i] = _scoreBoardScores[i + 1];
                }
            }
            else
            {
                for (int i = scoreIdx; i >= 0; i--)
                {
                    if (i == 0
                        || (HasScore(_scoreBoardPlayerIds[i - 1]) && !IsBetterScore(_scoreBoardScores[i - 1], score)))
                    {
                        _scoreBoardPlayerIds[i] = playerId;
                        _scoreBoardPlayerNames[i] = playerName;
                        _scoreBoardScores[i] = score;

                        break;
                    }
                    _scoreBoardPlayerIds[i] = _scoreBoardPlayerIds[i - 1];
                    _scoreBoardPlayerNames[i] = _scoreBoardPlayerNames[i - 1];
                    _scoreBoardScores[i] = _scoreBoardScores[i - 1];
                }
            }

            return true;
        }








        const int SCORE_BOARD_ITEM_DATA_COUNT = 4;
        const string SCORE_BOARD_ITEM_DATA_SEPARATOR = "#";
        string ScoreBoardToString()
        {
            var res = string.Empty;
            for (int i = 0; i < _scoreBoardItemCount; i++)
            {
                if (!HasScore(_scoreBoardPlayerIds[i]))
                {
                    continue;
                }

                var player = VRCPlayerApi.GetPlayerById(_scoreBoardPlayerIds[i]);
                var isSamePlayer = player != null && player.displayName == _scoreBoardPlayerNames[i];
                if (i != 0)
                {
                    res += SCORE_BOARD_ITEM_DATA_SEPARATOR;
                }
                res += $"{i},{_scoreBoardPlayerIds[i]},{NetworkMessageUtility.EncodeFloat(_scoreBoardScores[i])},{NetworkMessageUtility.BoolToInt(isSamePlayer)}";
            }
            return res;
        }

        bool StringToScoreBoard(string scoresString)
        {
            ClearScoreBoardData();

            var scoreStringList = scoresString.Split(SCORE_BOARD_ITEM_DATA_SEPARATOR);
            for (int i = 0; i < scoreStringList.Length; i++)
            {
                var scoreInfoStrings = scoreStringList[i].Split(",");

                if (scoreInfoStrings.Length != SCORE_BOARD_ITEM_DATA_COUNT)
                {
                    return false;
                }

                if (!int.TryParse(scoreInfoStrings[0], out var idx))
                {
                    return false;
                }

                if (!int.TryParse(scoreInfoStrings[1], out var playerId))
                {
                    return false;
                }

                if (!int.TryParse(scoreInfoStrings[2], out var scoreAsInt))
                {
                    return false;
                }
                var score = NetworkMessageUtility.DecodeFloat(scoreAsInt);

                if (!int.TryParse(scoreInfoStrings[3], out var isSamePlayerAsInt))
                {
                    return false;
                }
                var playerName = string.Empty;
                if (NetworkMessageUtility.IntToBool(isSamePlayerAsInt))
                {
                    playerName = Utils.GetPlayerDisplayNameById(playerId);
                }

                _scoreBoardPlayerIds[idx] = playerId;
                _scoreBoardPlayerNames[idx] = playerName;
                _scoreBoardScores[idx] = score;
            }

            return true;
        }








        [UdonSynced, FieldChangeCallback(nameof(OnNetworkMessageReceived_ScoreBoard))]
        string _networkMessage_ScoreBoard;
        const string MESSAGE_DATA_SEPARATOR = ":";
        bool _syncScoreBoardRequested = false;
        const int REQUEST_SYNC_SCORE_BOARD_MESSAGE_DATA_COUNT = 2;
        const int SEND_SCORE_BOARD_MESSAGE_DATA_COUNT = 3;
        const int SEND_SINGLE_SCORE_MESSAGE_DATA_COUNT = 4;

        void SendNetwork_RequestSyncScoreBoard()
        {
            if (!_initialized)
            {
                return;
            }

            if (Networking.LocalPlayer != null && !Networking.IsOwner(Networking.LocalPlayer, gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            _networkMessage_ScoreBoard = string.Join(MESSAGE_DATA_SEPARATOR,
                new[] {
                    NetworkMessageUtility.GetRandomMessageId().ToString(),
                    ((int)ScoreBoardMessageType.REQUEST_SYNC_SCORE_BOARD).ToString()});
            _syncScoreBoardRequested = true;

            _Debug.Log($"send RequestSyncScoreBoard: {_networkMessage_ScoreBoard}");

            RequestSerialization();
        }

        void SendNetwork_SendScoreBoard()
        {
            if (!_initialized)
            {
                return;
            }

            if (Networking.LocalPlayer != null && !Networking.IsOwner(Networking.LocalPlayer, gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            var scoreBoardString = ScoreBoardToString();
            if (string.IsNullOrEmpty(scoreBoardString))
            {
                return;
            }
            _networkMessage_ScoreBoard = string.Join(MESSAGE_DATA_SEPARATOR,
                new[] {
                    NetworkMessageUtility.GetRandomMessageId().ToString(),
                    ((int)ScoreBoardMessageType.SEND_SCORE_BOARD).ToString(),
                    scoreBoardString});

            _Debug.Log($"send SendScoreBoard: {_networkMessage_ScoreBoard}");

            RequestSerialization();
        }

        public void SendNetwork_SendSingleScore(int playerId, float score, bool bestOnly)
        {
            if (!_initialized)
            {
                return;
            }

            if (Networking.LocalPlayer != null && !Networking.IsOwner(Networking.LocalPlayer, gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            _networkMessage_ScoreBoard = string.Join(MESSAGE_DATA_SEPARATOR,
                new[] {
                    NetworkMessageUtility.GetRandomMessageId().ToString(),
                    ((int)(bestOnly ? ScoreBoardMessageType.SEND_SINGLE_SCORE_BEST_ONLY : ScoreBoardMessageType.SEND_SINGLE_SCORE)).ToString(),
                    playerId.ToString(),
                    NetworkMessageUtility.EncodeFloat(score).ToString()});

            _Debug.Log($"send SendSingleScore: {_networkMessage_ScoreBoard}");

            RequestSerialization();
        }

        string OnNetworkMessageReceived_ScoreBoard
        {
            set
            {
                if (!_initialized)
                {
                    return;
                }

                _networkMessage_ScoreBoard = value;
                _Debug.Log($"received ScoreBoard: {value}");

                if (string.IsNullOrEmpty(_networkMessage_ScoreBoard))
                {
                    return;
                }

                var messages = _networkMessage_ScoreBoard.Split(MESSAGE_DATA_SEPARATOR);

                if (messages.Length < 2)
                {
                    return;
                }

                if (!int.TryParse(messages[1], out var messageTypeInt))
                {
                    return;
                }

                switch ((ScoreBoardMessageType)messageTypeInt)
                {
                    case ScoreBoardMessageType.REQUEST_SYNC_SCORE_BOARD:
                        {
                            if (messages.Length != REQUEST_SYNC_SCORE_BOARD_MESSAGE_DATA_COUNT)
                            {
                                return;
                            }

                            if (!Utils.SafeGetIsMaster())
                            {
                                return;
                            }

                            SendNetwork_SendScoreBoard();

                            return;
                        }
                    case ScoreBoardMessageType.SEND_SCORE_BOARD:
                        {
                            if (messages.Length != SEND_SCORE_BOARD_MESSAGE_DATA_COUNT)
                            {
                                return;
                            }

                            if (!_syncScoreBoardRequested)
                            {
                                return;
                            }

                            if (string.IsNullOrEmpty(messages[2]) || !StringToScoreBoard(messages[2]))
                            {
                                return;
                            }

                            ApplyScoresToUI();
                            _syncScoreBoardRequested = false;

                            return;
                        }
                    case ScoreBoardMessageType.SEND_SINGLE_SCORE:
                    case ScoreBoardMessageType.SEND_SINGLE_SCORE_BEST_ONLY:
                        {
                            if (messages.Length != SEND_SINGLE_SCORE_MESSAGE_DATA_COUNT)
                            {
                                return;
                            }

                            if (!int.TryParse(messages[2], out var playerId))
                            {
                                return;
                            }

                            if (!int.TryParse(messages[3], out var scoreAsInt))
                            {
                                return;
                            }
                            float score = NetworkMessageUtility.DecodeFloat(scoreAsInt);

                            var bestOnly = (ScoreBoardMessageType)messageTypeInt == ScoreBoardMessageType.SEND_SINGLE_SCORE_BEST_ONLY;
                            var playerName = Utils.GetPlayerDisplayNameById(playerId);
                            UpdateScoreBoard(playerId, playerName, score, bestOnly, false);

                            return;
                        }
                }
            }
        }
    }








    public static class Utils
    {
        public static bool SafeGetIsMaster()
        {
            return Networking.LocalPlayer != null && Networking.LocalPlayer.isMaster;
        }

        public static string GetPlayerDisplayNameById(int playerId)
        {
            var player = VRCPlayerApi.GetPlayerById(playerId);
            return player != null ? player.displayName : string.Empty;
        }
    }

    public static class NetworkMessageUtility
    {
        const int NETWORK_ENCODE_FLOAT_TO_INT_DIGIT = 10000;

        public static int EncodeFloat(float v)
        {
            return (int)(v * NETWORK_ENCODE_FLOAT_TO_INT_DIGIT);
        }

        public static float DecodeFloat(int v)
        {
            return v / (float)NETWORK_ENCODE_FLOAT_TO_INT_DIGIT;
        }

        public static string EncodeVector3(ref Vector3 v)
        {
            return $"{EncodeFloat(v.x)},{EncodeFloat(v.y)},{EncodeFloat(v.z)}";
        }

        public static Vector3 DecodeVector3(int x, int y, int z)
        {
            return new Vector3(DecodeFloat(x), DecodeFloat(y), DecodeFloat(z));
        }

        public static int GetRandomSeed()
        {
            if (Networking.LocalPlayer == null)
            {
                return DateTime.Now.Millisecond;
            }

            return DateTime.Now.Millisecond + Networking.LocalPlayer.playerId * 1000;
        }

        public static int GetRandomMessageId()
        {
            return UnityEngine.Random.Range(0, 9999);
        }

        public static int BoolToInt(bool v)
        {
            return v ? 1 : 0;
        }

        public static bool IntToBool(int v)
        {
            return v == 1;
        }
    }

    public static class _Debug
    {
        public static void Log(object message)
        {
#if DEBUG_LOG
            Debug.Log(message);
#endif
        }
    }
}
