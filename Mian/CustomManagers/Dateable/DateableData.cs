namespace Topic_of_Love.Mian.CustomManagers.Dateable;
using System.Collections.Generic;
using System.ComponentModel;

public class DateableData : MetaObjectData
{
    private long _actor = -1;
    public List<long> _undateables = new List<long>();
    
    [DefaultValue(-1)]
    public long actor
    {
        get => _actor;
        set => _actor = value;
    }
    
    public override void Dispose()
    {
        _actor = 0L;
        _undateables.Clear();
        base.Dispose();
    }
}