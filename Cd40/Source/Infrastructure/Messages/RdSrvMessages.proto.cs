//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: RdSrvMessages.proto
// Note: requires additional types generated from: Common.proto
namespace U5ki.Infrastructure
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"RdSrvRxRs")]
  public partial class RdSrvRxRs : global::ProtoBuf.IExtensible
  {
    public RdSrvRxRs() {}
    
    private uint _ClkRate;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"ClkRate", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public uint ClkRate
    {
      get { return _ClkRate; }
      set { _ClkRate = value; }
    }
    private uint _ChannelCount;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"ChannelCount", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public uint ChannelCount
    {
      get { return _ChannelCount; }
      set { _ChannelCount = value; }
    }
    private uint _BitsPerSample;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"BitsPerSample", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public uint BitsPerSample
    {
      get { return _BitsPerSample; }
      set { _BitsPerSample = value; }
    }
    private uint _FrameTime;
    [global::ProtoBuf.ProtoMember(4, IsRequired = true, Name=@"FrameTime", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public uint FrameTime
    {
      get { return _FrameTime; }
      set { _FrameTime = value; }
    }
    private string _McastIp;
    [global::ProtoBuf.ProtoMember(5, IsRequired = true, Name=@"McastIp", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string McastIp
    {
      get { return _McastIp; }
      set { _McastIp = value; }
    }
    private uint _RdRxPort;
    [global::ProtoBuf.ProtoMember(6, IsRequired = true, Name=@"RdRxPort", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public uint RdRxPort
    {
      get { return _RdRxPort; }
      set { _RdRxPort = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"RdSrvTxRs")]
  public partial class RdSrvTxRs : global::ProtoBuf.IExtensible
  {
    public RdSrvTxRs() {}
    
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"RdSrvFrRs")]
  public partial class RdSrvFrRs : global::ProtoBuf.IExtensible
  {
    public RdSrvFrRs() {}
    

    private string _PttSrcId = "";
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"PttSrcId", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string PttSrcId
    {
      get { return _PttSrcId; }
      set { _PttSrcId = value; }
    }

    private U5ki.Infrastructure.RdSrvFrRs.SquelchType _Squelch = U5ki.Infrastructure.RdSrvFrRs.SquelchType.NoSquelch;
    [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"Squelch", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(U5ki.Infrastructure.RdSrvFrRs.SquelchType.NoSquelch)]
    public U5ki.Infrastructure.RdSrvFrRs.SquelchType Squelch
    {
      get { return _Squelch; }
      set { _Squelch = value; }
    }

    private uint _RtxGroupId = (uint)0;
    [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name=@"RtxGroupId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue((uint)0)]
    public uint RtxGroupId
    {
      get { return _RtxGroupId; }
      set { _RtxGroupId = value; }
    }

    private string _RtxGroupOwner = "";
    [global::ProtoBuf.ProtoMember(4, IsRequired = false, Name=@"RtxGroupOwner", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string RtxGroupOwner
    {
      get { return _RtxGroupOwner; }
      set { _RtxGroupOwner = value; }
    }

    private string _SqSite = "";
    [global::ProtoBuf.ProtoMember(5, IsRequired = false, Name=@"SqSite", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string SqSite
    {
      get { return _SqSite; }
      set { _SqSite = value; }
    }

    private string _ResourceId = "";
    [global::ProtoBuf.ProtoMember(6, IsRequired = false, Name=@"ResourceId", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string ResourceId
    {
      get { return _ResourceId; }
      set { _ResourceId = value; }
    }

    private string _QidxMethod = "";
    [global::ProtoBuf.ProtoMember(7, IsRequired = false, Name=@"QidxMethod", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string QidxMethod
    {
      get { return _QidxMethod; }
      set { _QidxMethod = value; }
    }

    private uint _QidxValue = default(uint);
    [global::ProtoBuf.ProtoMember(8, IsRequired = false, Name=@"QidxValue", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(uint))]
    public uint QidxValue
    {
      get { return _QidxValue; }
      set { _QidxValue = value; }
    }

    private U5ki.Infrastructure.RdSrvFrRs.FrequencyStatusType _FrequencyStatus = U5ki.Infrastructure.RdSrvFrRs.FrequencyStatusType.NotAvailable;
    [global::ProtoBuf.ProtoMember(9, IsRequired = false, Name=@"FrequencyStatus", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(U5ki.Infrastructure.RdSrvFrRs.FrequencyStatusType.NotAvailable)]
    public U5ki.Infrastructure.RdSrvFrRs.FrequencyStatusType FrequencyStatus
    {
      get { return _FrequencyStatus; }
      set { _FrequencyStatus = value; }
    }
    [global::ProtoBuf.ProtoContract(Name=@"SquelchType")]
    public enum SquelchType
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"NoSquelch", Value=0)]
      NoSquelch = 0,
            
      [global::ProtoBuf.ProtoEnum(Name=@"SquelchOnlyPort", Value=1)]
      SquelchOnlyPort = 1,
            
      [global::ProtoBuf.ProtoEnum(Name=@"SquelchPortAndMod", Value=2)]
      SquelchPortAndMod = 2
    }
  
    [global::ProtoBuf.ProtoContract(Name=@"FrequencyStatusType")]
    public enum FrequencyStatusType
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"NotAvailable", Value=0)]
      NotAvailable = 0,
            
      [global::ProtoBuf.ProtoEnum(Name=@"Available", Value=1)]
      Available = 1,
            
      [global::ProtoBuf.ProtoEnum(Name=@"Degraded", Value=2)]
      Degraded = 2
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"FrChangeResponse")]
  public partial class FrChangeResponse : global::ProtoBuf.IExtensible
  {
    public FrChangeResponse() {}
    
    private string _Frecuency;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"Frecuency", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string Frecuency
    {
      get { return _Frecuency; }
      set { _Frecuency = value; }
    }
    private bool _Set;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"Set", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public bool Set
    {
      get { return _Set; }
      set { _Set = value; }
    }
    private uint _Estado;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"Estado", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public uint Estado
    {
      get { return _Estado; }
      set { _Estado = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HFStatus")]
  public partial class HFStatus : global::ProtoBuf.IExtensible
  {
    public HFStatus() {}
    
    private U5ki.Infrastructure.HFStatusCodes _code;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"code", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public U5ki.Infrastructure.HFStatusCodes code
    {
      get { return _code; }
      set { _code = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"MNDisabledNodes")]
  public partial class MNDisabledNodes : global::ProtoBuf.IExtensible
  {
    public MNDisabledNodes() {}
    
    private readonly global::System.Collections.Generic.List<string> _nodes = new global::System.Collections.Generic.List<string>();
    [global::ProtoBuf.ProtoMember(1, Name=@"nodes", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<string> nodes
    {
      get { return _nodes; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"MSTransmiterInfo")]
  public partial class MSTransmiterInfo : global::ProtoBuf.IExtensible
  {
    public MSTransmiterInfo() {}
    
    private string _site;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"site", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string site
    {
      get { return _site; }
      set { _site = value; }
    }
    private string _txres;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"txres", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string txres
    {
      get { return _txres; }
      set { _txres = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"MSTransmittersStatus")]
  public partial class MSTransmittersStatus : global::ProtoBuf.IExtensible
  {
    public MSTransmittersStatus() {}
    
    private readonly global::System.Collections.Generic.List<U5ki.Infrastructure.MSTransmiterInfo> _nodes_info = new global::System.Collections.Generic.List<U5ki.Infrastructure.MSTransmiterInfo>();
    [global::ProtoBuf.ProtoMember(1, Name=@"nodes_info", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<U5ki.Infrastructure.MSTransmiterInfo> nodes_info
    {
      get { return _nodes_info; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
    [global::ProtoBuf.ProtoContract(Name=@"HFStatusCodes")]
    public enum HFStatusCodes
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"DISC", Value=0)]
      DISC = 0,
            
      [global::ProtoBuf.ProtoEnum(Name=@"NODISP", Value=1)]
      NODISP = 1,
            
      [global::ProtoBuf.ProtoEnum(Name=@"DISP", Value=2)]
      DISP = 2
    }
  
}