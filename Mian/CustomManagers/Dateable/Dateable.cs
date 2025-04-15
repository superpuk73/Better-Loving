namespace Topic_of_Love.Mian.CustomManagers.Dateable;

using System.Collections.Generic;

public class Dateable : MetaObject<DateableData>
{
    private List<Actor> _undateables = new List<Actor>();
    
    public Actor Actor => GetActor();
    
    public Actor GetActor()
    {
        return World.world.units.get(data.actor);
    }
    
    public void newDateable(Actor actor)
    {
        data.actor = actor.getID();
        Prepare();
    }
    
    public override void loadData(DateableData pData)
    {
        base.loadData(pData);
        Prepare();
    }
    
    public void Prepare()
    {
        _undateables.Clear();
        foreach (var id in data._undateables)
        {
            var actor = World.world.units.get(id);
            if (actor != null)
            {
                _undateables.Add(actor);
            }
        }
    }

    
    public void AddOrRemoveUndateable(Actor pActor)
    {
        if (data._undateables.Contains(pActor.getID()))
            data._undateables.Remove(pActor.getID());
        data._undateables.Add(pActor.getID());
        
        Prepare();
    }

    public List<Actor> GetUndateables()
    {
        return _undateables;
    }
    
    public bool IsUndateable(Actor pActor)
    {
        return _undateables.Contains(pActor);
    }
    
    public override void Dispose()
    {
        DBInserter.deleteData(getID(), "undateable");
        _undateables.Clear();
        base.Dispose();
    }
}