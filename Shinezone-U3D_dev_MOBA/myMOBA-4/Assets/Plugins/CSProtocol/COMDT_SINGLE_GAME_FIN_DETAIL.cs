using Assets.Scripts.Common;
using System;
using tsf4g_tdr_csharp;

namespace CSProtocol
{
	public class COMDT_SINGLE_GAME_FIN_DETAIL : ProtocolObject
	{
		public ProtocolObject dataObject;

		public static readonly uint BASEVERSION = 1u;

		public static readonly uint CURRVERSION = 1u;

		public static readonly int CLASS_ID = 181;

		public COMDT_SINGLE_GAME_PARAM_ADVENTURE stAdventure
		{
			get
			{
				return this.dataObject as COMDT_SINGLE_GAME_PARAM_ADVENTURE;
			}
			set
			{
				this.dataObject = value;
			}
		}

		public COMDT_SINGLE_COMBAT_ROBOT_DETAIL stCombat
		{
			get
			{
				return this.dataObject as COMDT_SINGLE_COMBAT_ROBOT_DETAIL;
			}
			set
			{
				this.dataObject = value;
			}
		}

		public COMDT_ACTIVITY_COMMON stActivity
		{
			get
			{
				return this.dataObject as COMDT_ACTIVITY_COMMON;
			}
			set
			{
				this.dataObject = value;
			}
		}

		public COMDT_BURNING_ENEMY_HERO_DETAIL stBurning
		{
			get
			{
				return this.dataObject as COMDT_BURNING_ENEMY_HERO_DETAIL;
			}
			set
			{
				this.dataObject = value;
			}
		}

		public ProtocolObject select(long selector)
		{
			if (selector <= 7L)
			{
				this.select_0_7(selector);
			}
			else if (this.dataObject != null)
			{
				this.dataObject.Release();
				this.dataObject = null;
			}
			return this.dataObject;
		}

		public TdrError.ErrorType construct(long selector)
		{
			TdrError.ErrorType result = TdrError.ErrorType.TDR_NO_ERROR;
			ProtocolObject protocolObject = this.select(selector);
			if (protocolObject != null)
			{
				return protocolObject.construct();
			}
			return result;
		}

		public TdrError.ErrorType pack(long selector, ref byte[] buffer, int size, ref int usedSize, uint cutVer)
		{
			if (buffer.GetLength(0) == 0 || size > buffer.GetLength(0))
			{
				return TdrError.ErrorType.TDR_ERR_INVALID_BUFFER_PARAMETER;
			}
			TdrWriteBuf tdrWriteBuf = ClassObjPool<TdrWriteBuf>.Get();
			tdrWriteBuf.set(ref buffer, size);
			TdrError.ErrorType errorType = this.pack(selector, ref tdrWriteBuf, cutVer);
			if (errorType == TdrError.ErrorType.TDR_NO_ERROR)
			{
				buffer = tdrWriteBuf.getBeginPtr();
				usedSize = tdrWriteBuf.getUsedSize();
			}
			tdrWriteBuf.Release();
			return errorType;
		}

		public TdrError.ErrorType pack(long selector, ref TdrWriteBuf destBuf, uint cutVer)
		{
			if (cutVer == 0u || COMDT_SINGLE_GAME_FIN_DETAIL.CURRVERSION < cutVer)
			{
				cutVer = COMDT_SINGLE_GAME_FIN_DETAIL.CURRVERSION;
			}
			if (COMDT_SINGLE_GAME_FIN_DETAIL.BASEVERSION > cutVer)
			{
				return TdrError.ErrorType.TDR_ERR_CUTVER_TOO_SMALL;
			}
			TdrError.ErrorType result = TdrError.ErrorType.TDR_NO_ERROR;
			ProtocolObject protocolObject = this.select(selector);
			if (protocolObject != null)
			{
				return protocolObject.pack(ref destBuf, cutVer);
			}
			return result;
		}

		public TdrError.ErrorType unpack(long selector, ref byte[] buffer, int size, ref int usedSize, uint cutVer)
		{
			if (buffer.GetLength(0) == 0 || size > buffer.GetLength(0))
			{
				return TdrError.ErrorType.TDR_ERR_INVALID_BUFFER_PARAMETER;
			}
			TdrReadBuf tdrReadBuf = ClassObjPool<TdrReadBuf>.Get();
			tdrReadBuf.set(ref buffer, size);
			TdrError.ErrorType result = this.unpack(selector, ref tdrReadBuf, cutVer);
			usedSize = tdrReadBuf.getUsedSize();
			tdrReadBuf.Release();
			return result;
		}

		public TdrError.ErrorType unpack(long selector, ref TdrReadBuf srcBuf, uint cutVer)
		{
			if (cutVer == 0u || COMDT_SINGLE_GAME_FIN_DETAIL.CURRVERSION < cutVer)
			{
				cutVer = COMDT_SINGLE_GAME_FIN_DETAIL.CURRVERSION;
			}
			if (COMDT_SINGLE_GAME_FIN_DETAIL.BASEVERSION > cutVer)
			{
				return TdrError.ErrorType.TDR_ERR_CUTVER_TOO_SMALL;
			}
			TdrError.ErrorType result = TdrError.ErrorType.TDR_NO_ERROR;
			ProtocolObject protocolObject = this.select(selector);
			if (protocolObject != null)
			{
				return protocolObject.unpack(ref srcBuf, cutVer);
			}
			return result;
		}

		private void select_0_7(long selector)
		{
			if (selector >= 0L && selector <= 7L)
			{
				switch ((int)selector)
				{
				case 0:
					if (!(this.dataObject is COMDT_SINGLE_GAME_PARAM_ADVENTURE))
					{
						if (this.dataObject != null)
						{
							this.dataObject.Release();
						}
						this.dataObject = (COMDT_SINGLE_GAME_PARAM_ADVENTURE)ProtocolObjectPool.Get(COMDT_SINGLE_GAME_PARAM_ADVENTURE.CLASS_ID);
					}
					return;
				case 1:
					if (!(this.dataObject is COMDT_SINGLE_COMBAT_ROBOT_DETAIL))
					{
						if (this.dataObject != null)
						{
							this.dataObject.Release();
						}
						this.dataObject = (COMDT_SINGLE_COMBAT_ROBOT_DETAIL)ProtocolObjectPool.Get(COMDT_SINGLE_COMBAT_ROBOT_DETAIL.CLASS_ID);
					}
					return;
				case 3:
					if (!(this.dataObject is COMDT_ACTIVITY_COMMON))
					{
						if (this.dataObject != null)
						{
							this.dataObject.Release();
						}
						this.dataObject = (COMDT_ACTIVITY_COMMON)ProtocolObjectPool.Get(COMDT_ACTIVITY_COMMON.CLASS_ID);
					}
					return;
				case 7:
					if (!(this.dataObject is COMDT_BURNING_ENEMY_HERO_DETAIL))
					{
						if (this.dataObject != null)
						{
							this.dataObject.Release();
						}
						this.dataObject = (COMDT_BURNING_ENEMY_HERO_DETAIL)ProtocolObjectPool.Get(COMDT_BURNING_ENEMY_HERO_DETAIL.CLASS_ID);
					}
					return;
				}
			}
			if (this.dataObject != null)
			{
				this.dataObject.Release();
				this.dataObject = null;
			}
		}

		public override int GetClassID()
		{
			return COMDT_SINGLE_GAME_FIN_DETAIL.CLASS_ID;
		}

		public override void OnRelease()
		{
			if (this.dataObject != null)
			{
				this.dataObject.Release();
				this.dataObject = null;
			}
		}
	}
}
