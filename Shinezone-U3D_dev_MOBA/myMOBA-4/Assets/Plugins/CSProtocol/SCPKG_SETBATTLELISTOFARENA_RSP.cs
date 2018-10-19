using Assets.Scripts.Common;
using System;
using tsf4g_tdr_csharp;

namespace CSProtocol
{
	public class SCPKG_SETBATTLELISTOFARENA_RSP : ProtocolObject
	{
		public byte bErrCode;

		public SCDT_SETBATTLELISTOFARENA_RESULT stResult;

		public static readonly uint BASEVERSION = 1u;

		public static readonly uint CURRVERSION = 1u;

		public static readonly int CLASS_ID = 1371;

		public SCPKG_SETBATTLELISTOFARENA_RSP()
		{
			this.stResult = (SCDT_SETBATTLELISTOFARENA_RESULT)ProtocolObjectPool.Get(SCDT_SETBATTLELISTOFARENA_RESULT.CLASS_ID);
		}

		public override TdrError.ErrorType construct()
		{
			return TdrError.ErrorType.TDR_NO_ERROR;
		}

		public TdrError.ErrorType pack(ref byte[] buffer, int size, ref int usedSize, uint cutVer)
		{
			if (buffer == null || buffer.GetLength(0) == 0 || size > buffer.GetLength(0))
			{
				return TdrError.ErrorType.TDR_ERR_INVALID_BUFFER_PARAMETER;
			}
			TdrWriteBuf tdrWriteBuf = ClassObjPool<TdrWriteBuf>.Get();
			tdrWriteBuf.set(ref buffer, size);
			TdrError.ErrorType errorType = this.pack(ref tdrWriteBuf, cutVer);
			if (errorType == TdrError.ErrorType.TDR_NO_ERROR)
			{
				buffer = tdrWriteBuf.getBeginPtr();
				usedSize = tdrWriteBuf.getUsedSize();
			}
			tdrWriteBuf.Release();
			return errorType;
		}

		public override TdrError.ErrorType pack(ref TdrWriteBuf destBuf, uint cutVer)
		{
			if (cutVer == 0u || SCPKG_SETBATTLELISTOFARENA_RSP.CURRVERSION < cutVer)
			{
				cutVer = SCPKG_SETBATTLELISTOFARENA_RSP.CURRVERSION;
			}
			if (SCPKG_SETBATTLELISTOFARENA_RSP.BASEVERSION > cutVer)
			{
				return TdrError.ErrorType.TDR_ERR_CUTVER_TOO_SMALL;
			}
			TdrError.ErrorType errorType = destBuf.writeUInt8(this.bErrCode);
			if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
			{
				return errorType;
			}
			long selector = (long)this.bErrCode;
			errorType = this.stResult.pack(selector, ref destBuf, cutVer);
			if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
			{
				return errorType;
			}
			return errorType;
		}

		public TdrError.ErrorType unpack(ref byte[] buffer, int size, ref int usedSize, uint cutVer)
		{
			if (buffer == null || buffer.GetLength(0) == 0 || size > buffer.GetLength(0))
			{
				return TdrError.ErrorType.TDR_ERR_INVALID_BUFFER_PARAMETER;
			}
			TdrReadBuf tdrReadBuf = ClassObjPool<TdrReadBuf>.Get();
			tdrReadBuf.set(ref buffer, size);
			TdrError.ErrorType result = this.unpack(ref tdrReadBuf, cutVer);
			usedSize = tdrReadBuf.getUsedSize();
			tdrReadBuf.Release();
			return result;
		}

		public override TdrError.ErrorType unpack(ref TdrReadBuf srcBuf, uint cutVer)
		{
			if (cutVer == 0u || SCPKG_SETBATTLELISTOFARENA_RSP.CURRVERSION < cutVer)
			{
				cutVer = SCPKG_SETBATTLELISTOFARENA_RSP.CURRVERSION;
			}
			if (SCPKG_SETBATTLELISTOFARENA_RSP.BASEVERSION > cutVer)
			{
				return TdrError.ErrorType.TDR_ERR_CUTVER_TOO_SMALL;
			}
			TdrError.ErrorType errorType = srcBuf.readUInt8(ref this.bErrCode);
			if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
			{
				return errorType;
			}
			long selector = (long)this.bErrCode;
			errorType = this.stResult.unpack(selector, ref srcBuf, cutVer);
			if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
			{
				return errorType;
			}
			return errorType;
		}

		public override int GetClassID()
		{
			return SCPKG_SETBATTLELISTOFARENA_RSP.CLASS_ID;
		}

		public override void OnRelease()
		{
			this.bErrCode = 0;
			if (this.stResult != null)
			{
				this.stResult.Release();
				this.stResult = null;
			}
		}

		public override void OnUse()
		{
			this.stResult = (SCDT_SETBATTLELISTOFARENA_RESULT)ProtocolObjectPool.Get(SCDT_SETBATTLELISTOFARENA_RESULT.CLASS_ID);
		}
	}
}
