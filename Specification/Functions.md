# Functions

Functions are sets of relations.

## Sets as functions

For the purpose of applying a set to an argument, all the set's members that are not relations are promoted to relations by substituting identity relations.

    PromoteToRelations = Sets set => set.Map { Relations r -> r, v -> (v->v) }
    // Relations is the set of all relations

    Application = (Sets set, arg) => Apply(PromoteToRelations set,arg) 


