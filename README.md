# MidOS

MidOS is a virtual operating system built on top of a custom-designed virtual machine, written in C#. The project simulates core OS concepts — process scheduling, virtual memory, memory management, and inter-process synchronization — all running on an abstract CPU with a custom instruction set.

> **Note:** This is an ongoing project and will continue to receive updates.

---

## What It Does

MidOS boots up, loads one or more programs (written in a simple assembly-like language), and manages their execution. The virtual machine interprets a custom set of opcodes, while the OS layer handles everything from memory allocation to process scheduling.

At a high level, MidOS provides:

- **Program loading** — Parses and loads assembly-style `.txt` programs into virtual memory
- **CPU emulation** — Fetch-decode-execute loop with a 15-register architecture
- **Memory management** — Flat byte-addressable memory with `ReadByte`, `WriteByte`, `ReadInt32`, and `WriteInt32` operations
- **Process scheduling** — Priority-based and time-quantum-based scheduling
- **Virtual memory** — Per-process page tables mapping virtual to physical addresses
- **Mutual exclusion** — OS-provided locks (mutexes) for inter-process synchronization
- **Events** — Signaled/non-signaled event objects for process coordination
- **I/O services** — Console input and output via dedicated opcodes

---

## Architecture

### Virtual Machine

The VM consists of three core components:

| Component | File | Responsibility |
|---|---|---|
| `CPU` | `CPU.cs` | Fetch-decode-execute loop, register state, flag management |
| `MemoryManager` | `MemoryManager.cs` | Physical memory as a flat byte array with typed read/write ops |
| `Loader` | `Loader.cs` | Parses assembly source files and writes instructions into memory |

### Registers

The CPU has 15 general-purpose registers (`r1`–`r15`), with the following conventions:

| Register | Role |
|---|---|
| `r11` | Instruction Pointer (IP) |
| `r12` | Base address of the global data region |
| `r13` | Stack Pointer (SP) |

### Instruction Format

Every instruction is exactly **9 bytes**:

```
[ 1 byte: opcode ][ 4 bytes: arg1 ][ 4 bytes: arg2 ]
```

### Assembly Syntax

Programs are written in plain text files using a simple assembly-like syntax:

```asm
; This is a comment
movi r1, #10      ; Move immediate value 10 into r1
movi r2, #5       ; Move immediate value 5 into r2
addr r1, r2       ; r1 = r1 + r2
printr r1         ; Print the value in r1
exit              ; Halt
```

**Argument prefixes:**
- `r1`, `r2`, ... — Register reference
- `#10` — Immediate integer value
- `@a` — Character literal (uses ASCII value)
- Raw integers — Memory addresses

---

## Supported Opcodes

| Opcode | Description |
|---|---|
| `MOVI` | Move immediate value into register |
| `MOVR` | Copy register to register |
| `MOVMR` | Write register value to memory address |
| `INCR` | Increment register by 1 |
| `ADDI` | Add immediate value to register |
| `ADDR` | Add register to register |
| `PUSHI` | Push immediate value onto stack |
| `PUSHR` | Push register value onto stack |
| `POPR` | Pop value from stack into register |
| `CMPI` | Compare register to immediate (sets flags) |
| `CMPR` | Compare register to register (sets flags) |
| `JMP` / `JMPI` | Unconditional jump |
| `JMPA` | Jump to absolute address |
| `JLT` / `JLTI` | Jump if less than (sign flag) |
| `JE` / `JEI` | Jump if equal (zero flag) |
| `CALL` | Call subroutine, push return address |
| `RET` | Return from subroutine |
| `PRINTR` | Print register value to console |
| `SLEEP` | Suspend process for N cycles |
| `INPUT` / `INPUTC` | Read integer or character from console |
| `SETPRIORITY` | Adjust the current process's scheduling priority |
| `EXIT` | Terminate the current process |

---

## Process Memory Layout

Each process has a virtual address space divided into four regions:

```
[ Code ] [ Global Data (512 bytes) ] [ Heap (512 bytes) ] [ Stack ]
```

- **Code** — Loaded instructions
- **Global Data** — Zero-initialized on process creation; base address stored in `r12`
- **Heap** — Dynamically allocated via `Alloc` / `FreeMemory` opcodes
- **Stack** — Grows downward; `r13` (SP) points to the top

---

## Process Scheduling

- **Priority-based:** 32 priority levels; higher number = higher priority. The highest-priority eligible process always runs next.
- **Time quantum:** Each process runs for a maximum of 10 clock cycles before being preempted.
- **Idle process:** Loaded from `idle.txt`; runs when no other process is eligible. Has the lowest priority and a quantum of 5 cycles.
- A running process is preempted when it: exits, sleeps, blocks on a lock, incurs a page fault with no free memory, waits on an event, or exhausts its time quantum.

---

## Synchronization

**Locks (Mutexes):**  
The OS provides 10 built-in locks (numbered 1–10). A process acquires a lock with `AcquireLock` and releases it with `ReleaseLock`. If a lock is already held, the requesting process blocks. Priority inversion is handled by temporarily boosting the lock-holding process's priority.

**Events:**  
10 built-in events (numbered 1–10) with signaled/non-signaled states. A process waits on an event with `WaitEvent` and signals it with `SignalEvent`.

---

## Getting Started

### Requirements

- [.NET SDK](https://dotnet.microsoft.com/) (C#)
- Windows 10 or later

### Build & Run

```bash
dotnet build
dotnet run
```

To invoke the OS with programs:

```bash
MidOS <virtual_memory_size_bytes> <program1.txt> <program2.txt> ...
```

Example:

```bash
MidOS 65536 my_program.txt idle.txt
```

---

## Roadmap

- [x] CPU emulation (fetch-decode-execute)
- [x] Memory manager (flat physical memory)
- [x] Loader (assembly parser)
- [x] Basic opcode set
- [ ] Process control blocks (PCBs)
- [ ] Priority-based scheduler
- [ ] Time quantum preemption
- [ ] Virtual memory & page tables
- [ ] Dynamic memory allocation (`Alloc` / `FreeMemory`)
- [ ] Lock implementation
- [ ] Event implementation
- [ ] Shared memory regions
- [ ] Idle process
- [ ] Full process lifecycle (statistics on exit)

---

## License

TBD
