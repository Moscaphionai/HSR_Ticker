using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGroup : IEnumerable<Block>
{
    private readonly List<Block> _blocks = new();

    public void Add(Block block)
    {
        _blocks.Add(block);
    }

    public bool isWalkable
    {
        get
        {
            BlockRemainPatch groupInclude = BlockRemainPatch.None;
            
            foreach (var block in _blocks)
            {
                groupInclude |= block.remainPatch;
                if ((groupInclude & BlockRemainPatch.Walkable) == BlockRemainPatch.Walkable)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public void ClearNeighbours()
    {
        foreach (var block in _blocks)
        {
            block.neighbours.Clear();
        }
    }

    public void AddNeighbour(BlockGroup oth)
    {
        foreach (var block in _blocks)
        {
            if ((block.remainPatch & BlockRemainPatch.Walkable) == 0)
            {
                continue;
            }
            foreach (var o in oth._blocks)
            {
                if ((o.remainPatch & BlockRemainPatch.Walkable) !=0)
                {
                    block.neighbours.Add(o);
                }
            }
        }
    }
    public IEnumerator<Block> GetEnumerator()
    {
        return _blocks.GetEnumerator();
    }

    IEnumerator<Block> IEnumerable<Block>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}