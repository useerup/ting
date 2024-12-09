# Spec

See https://gist.github.com/jonsecchis/9d95bbc1780f38b42b0110d2488e77f3


    Ledger = List.Empty mandated:Lists.Of Operations

    main = List.Of string args --> 

    Ledger.Add(position,operation) 

    Positions = {[
        Shares : nat
        AverageShareCost : decimal
        Gains = [
            GrossGain:decimal
            NetGain:decimal
        ]
        Tax : decimal
        Total = Shares * AverageShareCost
    ]}

    Operation = {[
        Shares : int
        ShareCost : decimal
        Total = Shares * ShareCost
    ]}

    Operations = Lists.Of Operation





