
# OrigoDB is going async

This solution compares 4 different implementations:

* Actors model using Akka.net
* Semi actors model using the TPL Dataflow library
* Disruptor.NET - the NET port of the LMAX disruptor
* Simple TPL and ConcurrentQueues

