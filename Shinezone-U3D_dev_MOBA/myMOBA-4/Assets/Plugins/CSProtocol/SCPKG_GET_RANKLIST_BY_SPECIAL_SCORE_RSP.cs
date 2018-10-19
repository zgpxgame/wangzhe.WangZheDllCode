using Assets.Scripts.Common;
using System;
using tsf4g_tdr_csharp;

namespace CSProtocol
{
	public class SCPKG_GET_RANKLIST_BY_SPECIAL_SCORE_RSP : ProtocolObject
	{
		public byte bNumberType;

		public uint dwItemNum;

		public CSDT_RANKING_LIST_ITEM_INFO[] astItemDetail;

		public static readonly uint BASEVERSION = 1u;

		public static readonly uint CURRVERSION = 240u;

		public static readonly int CLASS_ID = 1224;

		public SCPKG_GET_RANKLIST_BY_SPECIAL_SCORE_RSP()
		{
			this.astItemDetail = new CSDT_RANKING_LIST_ITEM_INFO[100];
			for (int i = 0; i < 100; i++)
			{
				this.astItemDetail[i] = (CSDT_RANKING_LIST_ITEM_INFO)ProtocolObjectPool.Get(CSDT_RANKING_LIST_ITEM_INFO.CLASS_ID);
			}
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
			if (cutVer == 0u || SCPKG_GET_RANKLIST_BY_SPECIAL_SCORE_RSP.CURRVERSION < cutVer)
			{
				cutVer = SCPKG_GET_RANKLIST_BY_SPECIAL_SCORE_RSP.CURRVERSION;
			}
			if (SCPKG_GET_RANKLIST_BY_SPECIAL_SCORE_RSP.BASEVERSION > cutVer)
			{
				return TdrError.ErrorType.TDR_ERR_CUTVER_TOO_SMALL;
			}
			TdrError.ErrorType errorType = destBuf.writeUInt8(this.bNumberType);
			if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
			{
				return errorType;
			}
			errorType = destBuf.writeUInt32(this.dwItemNum);
			if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
			{
				return errorType;
			}
			if (100u < this.dwItemNum)
			{
				return TdrError.ErrorType.TDR_ERR_REFER_SURPASS_COUNT;
			}
			if ((long)this.astItemDetail.Length < (long)((ulong)this.dwItemNum))
			{
				return TdrError.ErrorType.TDR_ERR_VAR_ARRAY_CONFLICT;
			}
			int num = 0;
			while ((long)num < (long)((ulong)this.dwItemNum))
			{
				errorType = this.astItemDetail[num].pack(ref destBuf, cutVer);
				if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
				{
					return errorType;
				}
				num++;
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
			if (cutVer == 0u || SCPKG_GET_RANKLIST_BY_SPECIAL_SCORE_RSP.CURRVERSION < cutVer)
			{
				cutVer = SCPKG_GET_RANKLIST_BY_SPECIAL_SCORE_RSP.CURRVERSION;
			}
			if (SCPKG_GET_RANKLIST_BY_SPECIAL_SCORE_RSP.BASEVERSION > cutVer)
			{
				return TdrError.ErrorType.TDR_ERR_CUTVER_TOO_SMALL;
			}
			TdrError.ErrorType errorType = srcBuf.readUInt8(ref this.bNumberType);
			if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
			{
				return errorType;
			}
			errorType = srcBuf.readUInt32(ref this.dwItemNum);
			if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
			{
				return errorType;
			}
			if (100u < this.dwItemNum)
			{
				return TdrError.ErrorType.TDR_ERR_REFER_SURPASS_COUNT;
			}
			int num = 0;
			while ((long)num < (long)((ulong)this.dwItemNum))
			{
				errorType = this.astItemDetail[num].unpack(ref srcBuf, cutVer);
				if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
				{
					return errorType;
				}
				num++;
			}
			return errorType;
		}

		public override int GetClassID()
		{
			return SCPKG_GET_RANKLIST_BY_SPECIAL_SCORE_RSP.CLASS_ID;
		}

		public override void OnRelease()
		{
			this.bNumberType = 0;
			this.dwItemNum = 0u;
			if (this.astItemDetail != null)
			{
				for (int i = 0; i < this.astItemDetail.Length; i++)
				{
					if (this.astItemDetail[i] != null)
					{
						this.astItemDetail[i].Release();
						this.astItemDetail[i] = null;
					}
				}
			}
		}

		public override void OnUse()
		{
			if (this.astItemDetail != null)
			{
				for (int i = 0; i < this.astItemDetail.Length; i++)
				{
					this.astItemDetail[i] = (CSDT_RANKING_LIST_ITEM_INFO)ProtocolObjectPool.Get(CSDT_RANKING_LIST_ITEM_INFO.CLASS_ID);
				}
			}
		}
	}
}
