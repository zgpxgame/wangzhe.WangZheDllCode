using Assets.Scripts.Framework;
using Assets.Scripts.GameLogic.GameKernal;
using CSProtocol;
using System;
using UnityEngine;

namespace Assets.Scripts.GameLogic
{
	[FrameCSSYNCCommandClass(CSSYNC_TYPE_DEF.CSSYNC_CMD_MOVE)]
	public struct MoveDirectionCommand : ICommandImplement
	{
        // 绕Y轴旋转的角度
		public short Degree;
        // 移动指令的序号: "0 --> 255, 0 --> 255" 
		public byte nSeq;

		[FrameCommandCreator]
		public static IFrameCommand Creator(ref CSDT_GAMING_CSSYNCINFO msg)
		{
			FrameCommand<MoveDirectionCommand> frameCommand = FrameCommandFactory.CreateCSSyncFrameCommand<MoveDirectionCommand>();
			frameCommand.cmdData.Degree = msg.stCSSyncDt.stMove.nDegree;
			frameCommand.cmdData.nSeq = msg.stCSSyncDt.stMove.bSeq;
			return frameCommand;
		}

		public bool TransProtocol(FRAME_CMD_PKG msg)
		{
			return true;
		}

		public bool TransProtocol(CSDT_GAMING_CSSYNCINFO msg)
		{
			msg.stCSSyncDt.stMove.nDegree = this.Degree;
			msg.stCSSyncDt.stMove.bSeq = this.nSeq;
			return true;
		}

		public void OnReceive(IFrameCommand cmd)
		{
			Player player = Singleton<GamePlayerCenter>.GetInstance().GetPlayer(cmd.playerID);
			if (player != null && ActorHelper.IsHostCtrlActor(ref player.Captain))
			{
                // 本地用户的移动命令需要做一些特殊处理：
                // 缓存接收时间戳、计算平均接收延迟
				Singleton<GameInput>.instance.OnHostActorRecvMove((int)this.Degree);
				if ((int)nSeq < FrameSynchr.MoveCmdBuff_Size && nSeq >= 0)
				{
					Singleton<FrameSynchr>.GetInstance().m_MoveCMDReceiveTime[(int)nSeq] = (uint)(Time.realtimeSinceStartup * 1000f);
					uint num = Singleton<FrameSynchr>.GetInstance().m_MoveCMDReceiveTime[(int)this.nSeq] - Singleton<FrameSynchr>.GetInstance().m_MoveCMDSendTime[(int)this.nSeq];
					Singleton<FrameSynchr>.GetInstance().m_receiveMoveCmdtotalCount += 1uL;
					Singleton<FrameSynchr>.GetInstance().m_receiveMoveCmdAverage = (int)(((float)Singleton<FrameSynchr>.GetInstance().m_receiveMoveCmdAverage * (Singleton<FrameSynchr>.GetInstance().m_receiveMoveCmdtotalCount - 1f) + num) / Singleton<FrameSynchr>.GetInstance().m_receiveMoveCmdtotalCount);
					if (num > Singleton<FrameSynchr>.GetInstance().m_receiveMoveCmdMax)
					{
						Singleton<FrameSynchr>.GetInstance().m_receiveMoveCmdMax = num;
					}
				}
			}
		}

		public void Preprocess(IFrameCommand cmd)
		{
			Player player = Singleton<GamePlayerCenter>.GetInstance().GetPlayer(cmd.playerID);
			if (player != null && player.Captain)
			{
				player.Captain.handle.ActorControl.PreMoveDirection(cmd, this.Degree, (int)this.nSeq);
			}
		}

		public void ExecCommand(IFrameCommand cmd)
		{
			Player player = Singleton<GamePlayerCenter>.GetInstance().GetPlayer(cmd.playerID);
			if (player != null && player.Captain)
			{
				player.Captain.handle.ActorControl.CmdMoveDirection(cmd, (int)this.Degree);
				if (!player.m_bMoved)
				{
                    // 首次移动，广播事件
					player.m_bMoved = true;
					Singleton<EventRouter>.instance.BroadCastEvent<Player>(EventID.FirstMoved, player);
				}
				if (ActorHelper.IsHostCtrlActor(ref player.Captain) && 
                    (int)nSeq < FrameSynchr.MoveCmdBuff_Size && nSeq >= 0)
				{
                    // 本地用户的移动命令需要做一些特殊处理：
                    // 缓存处理移动时间戳、计算平均处理移动延迟
					Singleton<FrameSynchr>.GetInstance().m_MoveCMDReceiveTime[(int)this.nSeq] = (uint)(Time.realtimeSinceStartup * 1000f);
					uint num = (uint)(Time.realtimeSinceStartup * 1000f) - Singleton<FrameSynchr>.GetInstance().m_MoveCMDSendTime[(int)this.nSeq];
					Singleton<FrameSynchr>.GetInstance().m_ExecMoveCmdTotalCount += 1uL;
					Singleton<FrameSynchr>.GetInstance().m_execMoveCmdAverage = (int)(((float)Singleton<FrameSynchr>.GetInstance().m_execMoveCmdAverage * (Singleton<FrameSynchr>.GetInstance().m_ExecMoveCmdTotalCount - 1f) + num) / Singleton<FrameSynchr>.GetInstance().m_ExecMoveCmdTotalCount);
					if (num > Singleton<FrameSynchr>.GetInstance().m_execMoveCmdMax)
					{
						Singleton<FrameSynchr>.GetInstance().m_execMoveCmdMax = num;
					}
				}
			}
		}

		public void AwakeCommand(IFrameCommand cmd)
		{

		}
	}
}
