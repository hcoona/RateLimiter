# RateLimiter #

This is a C# port of modified version of [Guava RateLimiter](https://github.com/google/guava/blob/master/guava/src/com/google/common/util/concurrent/SmoothRateLimiter.java).

See the comments in Guava source code for further details.

## Getting Started ##

A rate limiter. Conceptually, a rate limiter distributes permits at a configurable rate.
Each `acquire()` blocks if necessary until a permit is available, and then takes it.
Once acquired, permits need not be released.

RateLimiter is **safe** for concurrent use: It will restrict the total rate of calls from all threads.
Note, however, that it does not guarantee fairness. 

As an example, imagine that we have a list of tasks to execute, but we don't want to submit more than 2 per second: 

```csharp
var rateLimiter = RateLimiter.create(2.0, new SystemStopwatchProvider()); // rate is "2 permits per second"
for (var task : tasks) {
    rateLimiter.acquire(); // may wait
    submit(task);
}
```

As another example, imagine that we produce a stream of data, and we want to cap it at 5kb per second.
This could be accomplished by requiring a permit per byte, and specifying a rate of 5000 permits per second: 

```csharp
IRateLimiter rateLimiter = RateLimiter.create(5000.0); // rate = 5000 permits per second
void submitPacket(byte[] packet) {
    rateLimiter.acquire(packet.length);
    networkService.send(packet);
}
```

It is important to note that the number of permits requested never affects the throttling of
the request itself (an invocation to `acquire(1)` and an invocation to `acquire(1000)` will
result in exactly the same throttling, if any), but it affects the throttling of the next request.
I.e., if an expensive task arrives at an idle RateLimiter, it will be granted immediately,
but it is the next request that will experience extra throttling, thus paying for the cost of the expensive task.

## Future ##

Add documents.

## Acknowledgement ##

[Guava](https://github.com/google/guava) is licensed in Apache License 2.0 which is compatible with version 3 of the GPL according to [this](https://apache.org/licenses/GPL-compatibility.html).
