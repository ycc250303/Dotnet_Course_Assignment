# 第四次课程作业

作业内容：将 `Assignment4` 中给出的示例代码逐个运行，并结合运行结果说明代码行为。

## 测试环境（Windows）

| 项目 | 说明 |
| ---- | ---- |
| 操作系统 | Windows 11（本机版本：`10.0.26200`） |
| .NET SDK | `9.0.301` |
| 目标框架 | `net9.0` |
| 运行方式 | 使用 `Assignment4/_runner` 将各目录 `Program.cs` 复制为入口后执行 `dotnet run`（部分示例需管道输入或较长超时，见各节说明） |

---

## 1 运行记录

> 以下按执行顺序，每运行一个示例后立即追加本节。

### 1.1 `01 线程同步 / 01 Mutex`

运行说明：原代码在获得互斥体后调用 `ReadLine()` 会阻塞，本机通过管道送入换行以模拟用户按回车，从而执行 `ReleaseMutex()`。

输出内容：

```text
Running!
```

代码分析：

- **构造参数**：`new Mutex(false, MutexName)` 中第一个参数 `initiallyOwned` 为 `false` 表示创建后当前线程不自动成为拥有者，需通过 `WaitOne` 获取；第二个参数为内核对象名，**同名 Mutex 在系统内唯一**，因此第二个进程/实例会竞争同一把锁。
- **获取与超时**：`WaitOne(TimeSpan.FromSeconds(5))` 在 5 秒内尝试成为拥有者。成功则返回 `true`，进入 `else` 分支打印 `Running!`；若另一进程已持有且超时仍未释放，返回 `false`，打印 `Second instance is running!`。这演示了“单实例程序”的常见写法。
- **`ReadLine()` 的作用**：在临界区内阻塞，模拟程序长时间占用互斥体；只有用户（或本报告中的管道输入）结束输入后，才会执行 `ReleaseMutex()`，否则互斥体一直被当前进程持有。
- **释放与清理**：`ReleaseMutex()` 必须与成功获取的线程配对调用，且**只能由当前拥有者释放**。`finally` 里 `Dispose()` 关闭句柄；若线程在持有 Mutex 时异常终止，可能进入“被抛弃的 Mutex”状态，其他等待方行为与平台有关——对应源码末尾作业思考点。
- **与 `lock` 的区别**：`Mutex` 是内核同步对象，可跨进程、可命名；`lock` 仅进程内、更轻量。本例重点在**跨进程互斥**而非单纯保护内存。

### 1.2 `01 线程同步 / 02 SemaphoreSlim`

输出内容：

```text
Thread 1 waits to access a database
Thread 2 waits to access a database
Thread 1 was granted an access to a database
Thread 5 waits to access a database
Thread 5 was granted an access to a database
Thread 3 waits to access a database
Thread 3 was granted an access to a database
Thread 2 was granted an access to a database
Thread 4 waits to access a database
Thread 6 waits to access a database
Thread 1 is completed
Thread 6 was granted an access to a database
Thread 2 is completed
Thread 4 was granted an access to a database
Thread 3 is completed
Thread 5 is completed
Thread 4 is completed
Thread 6 is completed
```

代码分析：

- **信号量语义**：`SemaphoreSlim(4)` 表示内部计数器初值为 4，等价于“最多 4 个并发槽位”。`Wait()` 将计数减 1（减到 0 时后续 `Wait` 阻塞）；`Release()` 将计数加 1，并唤醒等待线程。这与互斥体“同时仅 1 人”不同，适合**连接池、带宽、限流**等模型。
- **与 `Sleep(seconds)` 的配合**：每个线程在 `Wait()` 成功后占用槽位 `seconds` 秒（`2+2*i`），时间越长，越容易在日志里看到“后启动的线程仍在 `waits` 排队”的现象。
- **输出为何交错**：`WriteLine` 发生在 `Wait` 前后及 `Sleep` 前后，多个线程并发执行时，控制台行顺序**不保证**与线程编号一致；但逻辑上应满足：先出现 `was granted`，再 `is completed`，且任意时刻处于 `Sleep` 的线程数不超过 4（加上调度细节可能略有重叠感知）。
- **`Main` 立即退出**：主线程在 `for` 循环里只负责 `Start`，不 `Join`。由于子线程默认为**前台线程**，进程会等所有工作线程结束后才退出，因此仍能打完整段日志。
- **与 `Semaphore`（命名）对比**：`SemaphoreSlim` 为轻量级，适合进程内；跨进程限流需命名信号量等机制。

### 1.3 `01 线程同步 / 03 AutoResetEvent`

输出内容：

```text
Waiting for another thread to complete work
Starting a long running work...
Work is done!
Waiting for a main thread to complete its work
First operation is completed!
Performing an operation on a main thread
Now running the second operation on a second thread
Starting second operation...
Work is done!
Second operation is completed!
```

代码分析：

- **两个 `AutoResetEvent` 的分工**：`_workerEvent` 表示“工作线程某一阶段是否做完”；`_mainEvent` 表示“主线程是否允许工作线程进入下一阶段”。二者把**异步协作**拆成两个方向的“发令—等待”，避免忙等。
- **`AutoResetEvent` 的关键性质**：`Set()` 后，**恰好一个**正在 `WaitOne()` 的线程被唤醒，随后事件自动回到非信号状态。若无人等待，`Set()` 的“信号”可能被消耗或保留（实现上表现为：下一次 `WaitOne` 可能立即通过），因此本例适合**严格交替**的握手，而不是广播。
- **执行顺序（与输出逐行对应）**：
  1. 主线程打印 `Waiting...` 后 `_workerEvent.WaitOne()` 阻塞。
  2. 子线程打印 `Starting a long running work...`，`Sleep(10)` 后 `Work is done!`，再 `_workerEvent.Set()` → 主线程被唤醒。
  3. 子线程随即 `_mainEvent.WaitOne()` 阻塞（对应 `Waiting for a main thread...`）。
  4. 主线程打印 `First operation is completed!`，`Sleep(5)`，`_mainEvent.Set()`，子线程继续第二阶段。
  5. 子线程再次 `Set` `_workerEvent`，主线程第二次 `WaitOne` 等到后打印 `Second operation is completed!`。
- **若改成 `ManualResetEvent`**：一次 `Set` 可能唤醒多个等待者，或需要额外 `Reset` 才能复刻同样协议，说明**事件类型决定协作协议**。

> 说明：更换示例后使用 `dotnet clean` 再 `dotnet run`，避免复用旧程序集导致输出与源码不一致。

### 1.4 `01 线程同步 / 04 ManualResetEventSlim`

输出内容：

```text
Thread 1 falls to sleep
Thread 3 falls to sleep
Thread 2 falls to sleep
Thread 1 waits for the gates to open!
The gates are now open!
Thread 1 enters the gates!
Thread 2 waits for the gates to open!
Thread 2 enters the gates!
The gates have been closed!
Thread 3 waits for the gates to open!
The gates are now open for the second time!
Thread 3 enters the gates!
The gates have been closed!
```

代码分析：

- **`ManualResetEventSlim` 与 `AutoResetEvent` 的差异**：`Set()` 后，所有阻塞在 `Wait()` 上的线程都会被放行，且事件保持“有信号”状态，直到显式 `Reset()`。因此适合“广播：闸门打开，大家一起过”，而不是“只唤醒一个”。
- **时间线与三个子线程**：
  - `Thread 1` 先睡 5s、`Thread 2` 睡 6s、`Thread 3` 睡 12s；之后各自打印 `waits for the gates` 并阻塞在 `_mainEvent.Wait()`。
  - 主线程 `Sleep(6)` 后打印 `The gates are now open!` 并 `Set()`：此时 **1、2 已到达闸门**（3 可能还在前 12s 睡眠中），故 1、2 进入；随后 `Sleep(2)` + `Reset()`，闸门关闭。
  - `Thread 3` 第一次 `Set` 时若仍在睡眠，则不会与其他线程一起通过；主线程再 `Sleep(10)` 后第二次 `Set()`，`Thread 3` 才打印 `enters`。
- **`Reset()` 的意义**：把事件拉回无信号状态，使**后到达** `Wait()` 的线程再次阻塞，从而可以模拟“闸门只开放一段时间”。
- **`Slim` 版本**：`ManualResetEventSlim` 在用户态混合自旋等待，短等待时比老 `ManualResetEvent` 更省上下文切换。

### 1.5 `01 线程同步 / 05 CountdownEvent`

输出内容：

```text
Starting two operations
Operation 1 is completed
Operation 2 is completed
Both operations have been completed.
```

代码分析：

- **计数语义**：`new CountdownEvent(2)` 将内部计数初始化为 2。每次 `Signal()`（或 `TrySignal`）使计数减 1；当计数到 0 时，所有阻塞在 `Wait()` 上的线程被唤醒。可把它理解为**可重复初始化的“还剩几件事没做完”**（与 `Barrier` 每阶段自动递增阶段号不同）。
- **与 `Join` 的对比**：这里用 `CountdownEvent` 把“两个异步操作的完成”抽象成**同一个同步点**；若只有两个线程，也可分别 `Join`，但 `CountdownEvent` 可扩展到 N 个任务、且不要求持有 `Thread` 引用（例如线程池工作项）。
- **本次输出顺序**：`Operation 1` 延时 4s、`Operation 2` 延时 8s，故先打印前者；但 `Both operations have been completed.` 一定在**两次 `Signal` 都发生之后**，即不早于较慢任务（8s）完成。
- **`Dispose()`**：`CountdownEvent` 实现 `IDisposable`，用完释放内部事件对象，避免句柄泄漏（在示例末尾已调用）。

### 1.6 `01 线程同步 / 06 Barrier`

输出内容（节选；完整运行约 296 秒，共 29 个阶段）：

```text
----------------------------------------------
----------------------------------------------
the singer starts to sing his song
the singer finishes to sing his song
the guitarist starts to play an amazing solo
the guitarist finishes to play an amazing solo
End of phase 1
...（phase 2～28 结构相同）...
End of phase 29
```

代码分析：

- **屏障（Barrier）模型**：`Barrier(2, postPhaseAction)` 表示有 **2 个参与者**。每个参与者每轮执行到 `SignalAndWait()` 时：若另一人未到，则**阻塞**；若两人都已到达，则**同时进入下一阶段**，并**恰好执行一次** `postPhaseAction`（回调里 `b.CurrentPhaseNumber + 1` 为面向用户的“阶段编号”）。
- **谁决定每轮墙钟时间**：吉他手每轮两次 `Sleep(5)`，歌手两次 `Sleep(2)`，**慢的一方决定整轮结束时刻**；快的一方会在 `SignalAndWait` 上等待，因此日志里总是两人各完成“开始/结束”后才出现 `End of phase N`。
- **与 `CountdownEvent` 的差异**：`CountdownEvent` 强调“N 个信号凑齐一次汇合”；`Barrier` 强调**多轮、同相位**的重复同步，适合并行计算中的 **phase barrier**（每阶段全员到齐再进入下一阶段）。
- **运行时长**：`for (int i = 1; i < 30; i++)` 实际 **29 轮**，每轮约数十秒，总时间可达数分钟；课堂演示可改小循环上界。
- **健壮性提示**：若某一参与线程在屏障上永久阻塞或异常退出，另一线程可能一直卡在 `SignalAndWait`；生产环境应考虑超时或取消。

### 1.7 `01 线程同步 / 07 ReaderWriterLockSlim`

源码说明：原文件在类定义结束后有一行作业文字，会导致编译失败；已改为 `//` 注释以便可编译运行（仅影响构建，不改变示例逻辑）。

输出内容（节选；主线程约 30 秒后退出，读线程为后台线程随之结束，日志量很大）：

```text
Reading contents of a dictionary
Reading contents of a dictionary
Reading contents of a dictionary
New key 49 is added to a dictionary by a Thread 1
In Reading
In Reading
In Reading
New key 35 is added to a dictionary by a Thread 2
...
```

代码分析：

- **线程角色**：3 个读线程 + 2 个写线程，全部 `IsBackground = true`。主线程仅 `Sleep(30)`；当主线程结束且前台线程不存在时，**进程退出，后台线程被终止**，因此读线程的 `while (true)` 不会无限跑下去——这是演示程序常用的“定时收工”写法。
- **读路径**：`EnterReadLock` 后遍历 `_items.Keys`，并在循环内 `Sleep(0.1)` 拉长持锁时间，**放大读写交错时的锁行为**（教学目的）。`finally` 中 `ExitReadLock` 保证异常时也能释放。
- **写路径与可升级锁**：写线程先 `EnterUpgradeableReadLock`，在**仍持有升级锁**的情况下检查 `ContainsKey`；仅当需要插入时才 `EnterWriteLock`。这样可以把“纯读检查”放在较轻的锁模式下，**减少写锁被不必要占用的时间**。注释中说明了：同时只有一个线程可处于可升级模式，且与写锁等待关系有关——适合“先读后写、但多数时候不写”的模式。
- **为何不用直接 `EnterWriteLock`**：若从头到尾用写锁，则与读线程完全互斥，读并发优势消失；可升级锁在“写少、且写之前要先读判断”时更贴合场景。
- **字典与枚举**：在 `foreach (var key in _items.Keys)` 期间持有读锁，避免枚举时集合被另一写线程修改导致未定义行为（在普通 `Dictionary` 上尤其危险）。

### 1.8 `01 线程同步 / 08 SpinWait`

输出内容（本次运行）：

```text
Running user mode waiting

UserModeWait Waiting is complete: 21220511531
Running hybrid SpinWait construct waiting
HybridSpinWait Waiting is complete : 747
Main end
```

代码分析：

- **测试设计**：`Main` 分两段各约 10s：第一段只跑 `UserModeWait`（纯 `while (!_isCompleted)`），第二段跑 `HybridSpinWait`（`SpinWait.SpinOnce()`）。两段之间把 `_isCompleted` 置 `true` 再置 `false`，相当于**重置计时窗口**。
- **纯自旋的问题**：循环体内几乎不做工作，CPU 会持续在用户态空转，因此 10s 内自增次数可达数百亿量级（本次约 `2.1e10`），对功耗与其他线程不友好，也**不能**代表“业务处理更快”。
- **`SpinWait` 的策略**：`SpinOnce` 随调用次数增加，会从**纯自旋**过渡到 **`Thread.SpinWait` / 让出时间片（yield）** 等，使线程在长时间等待时把 CPU 让出来。故同样 10s 内循环次数只有数百（本次 `747`），数量级差异体现的是**调度退让**，不是吞吐量提升。
- **`volatile`**：保证 `_isCompleted` 的读写不被编译器过度优化到寄存器缓存，多线程下能及时看到主线程的赋值。
- **适用场景**：等待时间**极短**且竞争窗口极短时，短暂自旋可避免上下文切换；等待时间长应使用 `Monitor.Wait`、事件或 `async/await`。

### 1.9 `02 线程池 / 01 APM 异步编程模型`

输出内容（节选；在 `net9.0` 下未能跑完）：

```text
Starting...
Is thread pool thread: False
Thread id: 4
Unhandled exception. System.PlatformNotSupportedException: Operation is not supported on this platform.
   at Program.RunOnThreadPool.BeginInvoke(...)
```

代码分析：

- **第一段：`new Thread(() => Test(out threadId))`**：在普通线程上执行 `Test`，`CurrentThread.IsThreadPoolThread` 为 `false`，用于与线程池对比。`out threadId` 把执行线程的 `ManagedThreadId` 传回主线程。
- **第二段：APM 意图**：`RunOnThreadPool` 委托指向同一 `Test` 方法，计划通过 `BeginInvoke` 异步启动、`AsyncWaitHandle.WaitOne()` 等待完成、`EndInvoke` 取返回值与 `out` 参数，并在 `Callback` 中打印 `AsyncState` 与回调线程信息——这是 .NET Framework 时代典型的 **Asynchronous Programming Model**。
- **在 `net9.0` 上的实际行为**：运行时在 `BeginInvoke` 处抛出 **`PlatformNotSupportedException`**。原因是现代 .NET 移除了“任意委托异步调用”所依赖的线程池路径，以避免跨平台语义不一致与维护成本。
- **迁移思路**：用 `Task.Run(() => Test(out threadId))` 或 `ThreadPool.QueueUserWorkItem` 包装；需要回调时用 `ContinueWith` 或 `async/await`；需要 `out` 参数时可改为返回元组或强类型结果对象。

### 1.10 `02 线程池 / 02 QueueUserWorkItem`

输出内容（节选，含可空引用警告）：

```text
Operation state: (null)
Worker thread id: 4
Operation state: async state
Worker thread id: 6
Operation state: lambda state me
Worker thread id: 4
Operation state: 3, lambda state 2
Worker thread id: 7
```

代码分析：

- **`WaitCallback` 签名**：`void WaitCallback(object state)`。第一个 `QueueUserWorkItem(AsyncOperation)` 未传 `state`，池线程收到 `state == null`，故输出 `(null)`。
- **显式状态**：`QueueUserWorkItem(AsyncOperation, "async state")` 把字符串传入，避免闭包分配，适合简单数据透传。
- **Lambda 与闭包**：`state => { ... }` 形式仍使用 `QueueUserWorkItem` 的第二个参数作为 `state`；另一个 lambda 使用 `_ =>` 忽略参数，但通过闭包读取 `x,y,lambdaState`——展示**捕获局部常量/外层变量**的写法（捕获引用类型时要注意生命周期与线程安全）。
- **调度与顺序**：四个任务提交后，主线程穿插 `Sleep(1s)`、`Sleep(2s)`，仅控制**提交节奏**，不保证完成顺序；`Worker thread id` 在不同行可能重复（池线程复用）或不同（多 worker），均属正常。
- **`CS8622` 警告**：启用可空引用后，`AsyncOperation(object state)` 与 `WaitCallback` 的 null 性注解不完全匹配，属静态分析提示；运行时本示例行为不受影响。可将参数改为 `object?` 或显式包装以消除警告。

### 1.11 `02 线程池 / 03 线程池并行度`（源码文件 `ProgramRun.cs`）

输出内容（节选；含大量线程 ID 打印）：

```text
Scheduling work by creating threads
4,5,6,...503,
Execution time using threads: 150
Starting work on a threadpool
504,506,...524,
Execution time using the thread pool: 2705
```

代码分析：

- **`CountdownEvent` 的作用**：`numberOfOperations = 500` 时，创建 500 个工作单元，每个单元在结束时 `Signal()`，`countdown.Wait()` 保证**全部完成**后主线程才继续计时，避免“主线程先打印耗时、工作还没跑完”的测量错误。
- **`UseThreads` 为何墙钟很短（本次约 150ms）**：每个线程体内仅 `Sleep(0.1s)`，500 个线程**并行**休眠，理想情况下墙钟接近单次 `Sleep` 的量级（再加创建线程与调度开销）。代价是**500 个线程对象**与内核线程资源，扩展性极差。
- **`UseThreadPool` 为何更慢（本次约 2705ms）**：线程池**工作线程数量有限**，500 个委托进入全局队列，由少量 worker **串行/分批**执行；每个工作项仍 `Sleep(0.1s)`，整体墙钟近似“队列长度 / 并行度 × 0.1s”的量级（还受 `Write` 同步输出影响）。这说明了线程池的优化目标是**资源可控与吞吐稳定**，不是让“500 个同时睡 0.1s”的伪并行更快。
- **控制台 `Write` 的影响**：`Write($"{ManagedThreadId},")` 大量输出会产生锁竞争，可能放大两种方式的耗时差异；微基准时应减少 I/O 或使用无锁聚合。
- **结论**：该示例是**教学对比**而非“证明线程池更快”；读结果要结合任务是否 CPU 密集、是否适合并行、以及池的并发度设置（`ThreadPool.SetMinThreads` 等，本例未用）。

### 1.12 `02 线程池 / 04 取消线程池中的操作`

输出内容：

```text
Starting the first task
Starting the second task
The first task has been canceled.
Starting the third task
The second task has been canceled.
The third task has been canceled.
```

代码分析：

- **协作式取消模型**：`CancellationToken` 不会“强行杀死”线程；工作代码必须在循环或阻塞点**主动检查**取消，才能及时收尾。这与 `Thread.Abort` 一类粗暴终止有本质区别。
- **三种写法对比**：
  1. **`IsCancellationRequested` 轮询**：最直观，适合已有循环结构；可在取消时做清理再 `return`。
  2. **`ThrowIfCancellationRequested`**：取消时抛 `OperationCanceledException`，适合与 `try/catch` 或 TPL 统一处理取消异常；注意异常控制流成本。
  3. **`Register` 回调**：在取消瞬间把 `cancellationFlag` 设为 `true`；工作循环仍要轮询标志，本质仍是协作式。`Register` 还可用于释放资源、唤醒等待等（本例仅演示标志位）。
- **主线程时序**：每个 `using (var cts = ...)` 块内：`QueueUserWorkItem` 启动任务 → `Sleep(2)` → `Cancel()`。任务每轮 `Sleep(1)`，因此在第 2～3 轮之间会观察到取消。
- **输出交错**：`Starting the second task` 与第一个任务的取消信息可能交错打印，因多个池任务与控制台输出并发，**不代表逻辑错误**。
- **`Sleep(2)` 在末尾**：给最后一个任务的取消处理留出完成时间，避免进程过早退出看不到尾部输出。

### 1.13 `03 并发集合 / 01 ConcurrentDictionary`（`Program.cs`）

输出内容（本次运行，单线程各 1000 万次操作）：

```text
Writing to dictionary with a    lock: 00:00:00.3164931
Writing to a concurrent   dictionary: 00:00:01.7906199
Reading from dictionary with a  lock: 00:00:00.1669117
Reading from a concurrent dictionary: 00:00:00.0961039
```

代码分析：

- **基准结构**：`Iterations = 10_000_000`，依次测四种操作：① `Dictionary` 写入（每次 `lock`）；② `ConcurrentDictionary` 写入；③ `Dictionary` 读取（`lock`）；④ `ConcurrentDictionary` 读取。单线程下无竞争，`lock` 退化为极低开销的互斥，主要比较的是**每次 API 的固定成本**。
- **写性能（本次）**：`Dictionary`+`lock` 约 **0.32s**，`ConcurrentDictionary` 约 **1.79s**。`ConcurrentDictionary` 的索引器写入需维护并发安全（例如分段锁、原子操作、内存屏障等），在**无竞争**时这些成本无法被“并发收益”摊薄，因此更慢是预期现象之一。
- **读性能（本次）**：`Dictionary`+`lock` 约 **0.17s**，`ConcurrentDictionary` 约 **0.10s**。全局锁在每次读取都要进入/退出临界区；`ConcurrentDictionary` 的读路径常经过优化（实现细节随版本变化），在本机上表现为读更快。
- **`CurrentItem` 字段**：用于防止编译器把纯读取优化掉（副作用），使读测试更贴近“真实使用会用到值”的场景；可能触发 `CS8618` 等可空警告，不影响运行逻辑。
- **结论**：不能把本示例推广为“并发字典读一定快、写一定慢”；只能说明**在单线程、极高次数循环**下，两类 API 的常数因子不同。多线程争用下需重新测量（见 1.14）。

### 1.14 `03 并发集合 / 01 ConcurrentDictionaryTest`（`ProgramRun.cs`）

输出内容：

```text
Running on a single core
Scheduling work by creating threads

Execution time using threads: 99
Scheduling work by creating threads

Execution time using the thread pool: 423
```

代码分析：

- **任务划分**：`numberOfOperations = 3`，`Iterations = 2_000_000`，每个线程负责连续区间 `interval = Iterations/numberOfOperations` 个键。键空间 `[0, 2_000_000)` 被均分给三个线程，理论上**无键冲突**，但 `Dictionary` 路径仍对**每一次写入**加全局锁，三个线程会**严重串行化**。
- **`ProcessorAffinity = 7`**：二进制 `111`，表示可在多个逻辑处理器上调度（具体核心数依机器而定）。亲和性会改变调度与缓存行为，与“单线程基准”不可直接横向比较。
- **第一段 `UseThreads_Dictionary`（本次约 99ms）**：源码在**每个键**的 `dictionary[i] = Item` 外都包了一层 `lock (dictionary)`，三线程在约 200 万次写入上会**极其频繁地抢同一把全局锁**，本质上是高度串行化。本次耗时仍较低，与机器性能、调度与缓存行为有关；若增大线程数、改为随机键写入或拉长临界区，锁争用成本通常会显著上升。
- **第二段 `UseThreads_ConcurrentDictionary`（本次约 423ms）**：无外层 `lock`，但 `ConcurrentDictionary` 每次写入仍有内部同步与数据结构维护成本；在“仅三线程、总写入量固定”时，**不一定**比粗粒度锁更快，甚至可能更慢（本次即如此）。
- **文案错误**：第二段仍打印 `Scheduling work by creating threads`，而耗时一行写 `using the thread pool`，与实现不符（两段均为 `new Thread`）。阅读与写报告应以实际调用的方法为准。
- **延伸**：若将 `numberOfOperations` 提到 6 或把写入改为随机键，两种结构的相对排名可能变化——**并发结构选型必须以工作负载与线程数实测为准**。

### 1.15 `03 并发集合 / 01 ConcurrentDictionaryTest`（`Program.cs`）

说明：`Program.cs` 与 `03 并发集合/01 ConcurrentDictionary/Program.cs` 内容一致，均为单线程 1000 万次读写对比，运行结果见 **1.13**，此处不重复执行。

与 **1.13** 的关系：同一套四段计时逻辑，便于在“并发集合目录”与“带 Test 后缀目录”两处对照阅读；分析要点（单线程下写慢于 `Dictionary+lock`、读可能更快）完全适用。

---

## 2 总结

1. **同步原语（按“适用问题”记）**
   - **`Mutex`**：命名内核对象，**跨进程**互斥；适合单实例、守护进程互斥；需严格配对 `ReleaseMutex`，并理解异常退出时的“被抛弃互斥体”问题。
   - **`SemaphoreSlim`**：**计数信号量**，控制“最多 N 个并发”；`Wait/Release` 必须配对，防止信号量泄漏。
   - **`AutoResetEvent`**：**唤醒一个**等待者并自动复位，适合点对点握手协议。
   - **`ManualResetEventSlim`**：**广播闸门**，`Set` 后多线程可同时通过，需 `Reset` 关闭；适合阶段性放行。
   - **`CountdownEvent`**：N 次 `Signal` 凑齐一次汇合；适合“多任务完成后再继续”。
   - **`Barrier`**：多参与者**多轮**同步，每轮全员到齐进入下一阶段；回调只在阶段推进时触发一次。
   - **`ReaderWriterLockSlim`**：读并发、写独占；**可升级读锁**适合“先读后写且写较少”的路径。
   - **`SpinWait`**：短等待优化手段，长等待应让出 CPU 或用阻塞原语。

2. **线程池与异步模型**
   - **`ThreadPool.QueueUserWorkItem`**：适合短任务；执行顺序与线程复用由 CLR 决定，不应依赖“先入先出”的打印顺序。
   - **`CancellationToken`**：**协作式**取消，需在工作代码中检查；三种示例对应轮询、异常与回调登记。
   - **委托 APM（`BeginInvoke`）**：在 **`net9.0` 不可用**（`PlatformNotSupportedException`），应迁移到 **`Task` / `async-await`**。

3. **性能与数据结构**
   - **线程池 ≠ 更快**：本作业中“500 个短 `Sleep`”场景下，池化反而墙钟更长，说明要区分**并行度、队列延迟、I/O 与锁**。
   - **`ConcurrentDictionary` ≠ 更快**：单线程高次循环下写路径常更慢；多线程下是否胜出取决于**争用模式、键分布、读写比**，必须以实测为准。
