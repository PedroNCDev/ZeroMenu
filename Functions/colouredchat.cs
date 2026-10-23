using System;
using UnityEngine;
using Il2CppInterop.Runtime; 
using Hazel;
public class ColouredChat
{
    public static void SendQuickChatById(int id)
    {
        try
        {
            if (PlayerControl.LocalPlayer == null) return;

          
            var writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)33, 
                SendOption.Reliable,
                -1
            );
            writer.Write((byte)3);
            writer.Write((ushort)78);   //im putting it here im too lazy to put it in chat.cs xdd
            writer.Write((byte)1);
            writer.Write((byte)2);
            writer.Write((ushort)id); 
            AmongUsClient.Instance.FinishRpcImmediately(writer);

           
            var selfWriter = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)33,
                SendOption.Reliable,
                (int)PlayerControl.LocalPlayer.OwnerId
            );
            selfWriter.Write((byte)3);
            selfWriter.Write((ushort)78);
            selfWriter.Write((byte)1);
            selfWriter.Write((byte)2);
            selfWriter.Write((ushort)id);
            AmongUsClient.Instance.FinishRpcImmediately(selfWriter);
        }
        catch (Exception ex)
        {
         
            Debug.LogError("Error en SendQuickChatById: " + ex.Message);
        }
    }
}
