using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using ZeroMenu.Utilities;

namespace ZeroMenu.Utilities
{
    public static class GameOptions
    {
        public static IGameOptions CreateCloneOptions(IGameOptions options)
        {
            LogicOptions logicOptions = GameManager.Instance.LogicOptions;
            byte[] byteArray = logicOptions.gameOptionsFactory.ToBytes(options, AprilFoolsMode.IsAprilFoolsModeToggledOn);
            return logicOptions.gameOptionsFactory.FromBytes(byteArray);
        }

        public static void SendGameOptionsToClient(IGameOptions options, int targetClientId)
        {
            if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay && targetClientId == PlayerControl.LocalPlayer.OwnerId)
            {
                GameManager.Instance.LogicOptions.SetGameOptions(options);
                return;
            }

            MessageWriter writer = MessageWriter.Get(SendOption.None);
            writer.StartMessage((byte)FindLogicOptionsIndex());
            writer.WriteBytesAndSize(GameManager.Instance.LogicOptions.gameOptionsFactory.ToBytes(options, AprilFoolsMode.IsAprilFoolsModeToggledOn));
            writer.EndMessage();

            BatchedMessage batch = new BatchedMessage(targetClientId);
            batch.QueueDataFlag(GameManager.Instance.NetId, writer);
            batch.FinishBatch();
        }

        private static int FindLogicOptionsIndex()
        {
            int logicIndex = -1;
            for (int i = 0; i < GameManager.Instance.LogicComponents.Count; i++)
            {
                GameLogicComponent component = GameManager.Instance.LogicComponents[i];
                if (component.GetType() != typeof(LogicOptions)) continue;

                logicIndex = i;
                break;
            }
            return logicIndex;
        }
    }
}
