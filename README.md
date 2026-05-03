# MidOS — Virtual Operating System

A fully functional virtual operating system and custom CPU simulator built in C# as a semester-long OS design project. MidOS simulates core operating system concepts including process scheduling, virtual memory, paging, heap allocation, mutual exclusion, and inter-process synchronization — all running on a custom-built virtual machine.

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [How to Run](#how-to-run)
- [Writing Programs](#writing-programs)
- [Instruction Set Reference](#instruction-set-reference)
- [Module Breakdown](#module-breakdown)
- [Test Programs](#test-programs)
- [Project Structure](#project-structure)

---

## Overview

MidOS is built across six progressive modules, each adding a new layer of OS functionality on top of the last:

| Module | Feature |
|--------|---------|
| 1 | Custom CPU with 14 registers, full instruction set, fetch-decode-execute loop |
| 2 | Memory paging with page table translation and page boundary handling |
| 3 | Process management, PCB, context switching, priority scheduling, sleep |
| 4 | Locks (mutexes), events, shared memory, priority inversion |
| 5 | Dynamic heap allocation with first-fit allocator |
| 6 | Virtual memory with LRU page eviction and dirty bit optimization |

---

## Architecture

```
Program.cs          ← Boot sequence, command-line parsing, process loading
    │
    ├── MemoryManager.cs   ← MMU: virtual address translation, paging, page fault handler
    │       └── VirtualPage.cs   ← Per-page metadata: IsValid, IsDirty, LastAccessed
    │
    ├── Loader.cs          ← Assembly parser: text → binary, memory layout, register init
    │
    ├── CPU.cs             ← Fetch-decode-execute engine, all opcodes, context switch
    │       └── Opcodes.cs       ← Enum of all 50+ instructions
    │
    ├── Scheduler.cs       ← OS kernel: scheduling loop, sleep countdown, lock wakeup,
    │                         priority inversion, exit reports
    │
    └── PCB.cs             ← Process Control Block: registers, flags, page table, metrics
            └── ProcessState.cs  ← New / Ready / Running / WaitingAsleep / WaitingLock /
                                   WaitingEvent / Terminated
```

### Memory Layout (per process)

```
┌─────────────────┐ ← startAddress (e.g. 8192 for Process 1)
│      Code       │   instructionCount × 9 bytes
├─────────────────┤
│      Stack      │   4096 bytes, grows downward from top
├─────────────────┤
│   Global Data   │   1024 bytes, zero-initialized
├─────────────────┤
│      Heap       │   512 bytes, managed by first-fit allocator
└─────────────────┘
```

### Virtual Address Space

```
Physical RAM:  256 pages × 256 bytes = 64 KB
Virtual Space: 512 pages × 256 bytes = 128 KB

Pages 0–255:   mapped 1:1 to physical RAM at startup (IsValid = true)
Pages 256–511: virtual only — access triggers a page fault (IsValid = false)
```

---

## Getting Started

### Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download) or later
- Windows 10 or later (required per project spec)

### Build

```bash
git clone https://github.com/yourusername/MidOS.git
cd MidOS/MID
dotnet build
```

---

## How to Run

```
dotnet run -- <memorySize> <program1.txt> [program2.txt] ...
```

| Argument | Description |
|----------|-------------|
| `memorySize` | Total virtual memory in bytes (e.g. `65536` for 64KB) |
| `program1.txt` | Path to first program file |
| `program2.txt` | Optional additional programs — loaded as concurrent processes |

The OS always loads `idle.txt` from the working directory as the idle process (priority 1, quantum 5). If the file does not exist it is created automatically.

### Examples

```bash
# Run a single program
dotnet run -- 65536 hello.txt

# Run two programs concurrently
dotnet run -- 65536 prog1.txt prog2.txt

# Run with larger memory
dotnet run -- 131072 prog1.txt prog2.txt prog3.txt
```

---

## Writing Programs

Programs are plain text files containing one assembly instruction per line. Comments start with `;`.

```asm
; hello.txt
movi r1, #72    ; 'H'
printcr r1
movi r1, #105   ; 'i'
printcr r1
movi r1, #10    ; newline
printcr r1
exit
```

### Argument Syntax

| Syntax | Meaning | Example |
|--------|---------|---------|
| `r1` – `r10` | General-purpose register | `movi r1, #5` |
| `#n` | Immediate integer constant | `addi r1, #10` |
| `@c` | Character constant (ASCII value) | `movi r1, @A` |
| `1234` | Raw memory address | `jmpa 1234` |

### Reserved Registers

| Register | Purpose |
|----------|---------|
| `r11` | Instruction Pointer (IP) — not directly accessible |
| `r12` | Current Process ID — read-only by convention |
| `r13` | Stack Pointer (SP) — managed by PUSH/POP |
| `r14` | Global Data base address — set by OS on load |

### Jump Offset Convention

All relative jump offsets are measured **from the next instruction**:

```
new IP = (current IP + 9) + offset
```

| Goal | Offset |
|------|--------|
| Skip 1 instruction forward | `#9` |
| Skip 2 instructions forward | `#18` |
| Loop back over 1 instruction | `#-18` |
| Loop back over 2 instructions | `#-27` |
| Loop back over 3 instructions | `#-36` |
| Loop back over N instructions | `#(-(N+1)*9)` |

---

## Instruction Set Reference

### Arithmetic

| Instruction | Effect |
|-------------|--------|
| `incr rx` | `rx ← rx + 1` |
| `addi rx, #n` | `rx ← rx + n` |
| `addr rx, ry` | `rx ← rx + ry` |

### Data Movement

| Instruction | Effect |
|-------------|--------|
| `movi rx, #n` | `rx ← n` |
| `movr rx, ry` | `rx ← ry` |
| `movmr rx, ry` | `rx ← [ry]` (memory at address in ry) |
| `movrm rx, ry` | `[rx] ← ry` (write ry to address in rx) |
| `movmm rx, ry` | `[rx] ← [ry]` (memory to memory) |

### Stack

| Instruction | Effect |
|-------------|--------|
| `pushr rx` | Push rx onto stack; SP -= 4 |
| `pushi #n` | Push constant onto stack; SP -= 4 |
| `popr rx` | Pop top of stack into rx; SP += 4 |
| `popm rx` | Pop top of stack into `[rx]`; SP += 4 |

### Control Flow

| Instruction | Effect |
|-------------|--------|
| `jmp rx` | Relative jump by rx bytes from next instruction |
| `jmpi #n` | Relative jump by n bytes from next instruction |
| `jmpa #n` | Absolute jump to address n |
| `call rx` | Push return address, relative jump by rx |
| `callm rx` | Push return address, relative jump by [rx] |
| `ret` | Pop return address and jump to it |
| `exit` | Terminate the process |
| `sleep rx` | Sleep rx clock cycles (0 = indefinite) |

### Comparisons and Conditional Jumps

| Instruction | Condition | Jump |
|-------------|-----------|------|
| `cmpi rx, #n` | Sets flags: sign if rx < n, zero if rx == n | — |
| `cmpr rx, ry` | Sets flags: sign if rx < ry, zero if rx == ry | — |
| `jlt rx` | Sign flag set | Relative by rx |
| `jlti #n` | Sign flag set | Relative by n |
| `jlta #n` | Sign flag set | Absolute to n |
| `jgt rx` | Sign clear AND zero clear | Relative by rx |
| `jgti #n` | Sign clear AND zero clear | Relative by n |
| `jgta #n` | Sign clear AND zero clear | Absolute to n |
| `je rx` | Zero flag set | Relative by rx |
| `jei #n` | Zero flag set | Relative by n |
| `jea #n` | Zero flag set | Absolute to n |

### I/O

| Instruction | Effect |
|-------------|--------|
| `printr rx` | Print integer value of rx |
| `printm rx` | Print integer at memory address in rx |
| `printcr rx` | Print rx as ASCII character |
| `printcm rx` | Print character at memory address in rx |
| `input rx` | Read integer from keyboard into rx |
| `inputc rx` | Read character from keyboard into rx (ASCII value) |

### Scheduling

| Instruction | Effect |
|-------------|--------|
| `setpriority rx` | Set process priority to value at `[rx]` (1–32) |
| `setpriorityi #n` | Set process priority to n (1–32) |

### Synchronization (Module 4)

| Instruction | Effect |
|-------------|--------|
| `acquirelock rx` | Acquire OS lock # in rx (1–10). Blocks if held. |
| `acquirelocki #n` | Acquire OS lock n. |
| `releaselock rx` | Release OS lock # in rx if owned by this process. |
| `releaselocki #n` | Release OS lock n. |
| `signalevent rx` | Signal event # in rx (1–10). |
| `signaleventi #n` | Signal event n. |
| `waitevent rx` | Block until event # in rx is signaled. |
| `waiteventi #n` | Block until event n is signaled. |

### Shared Memory (Module 4)

| Instruction | Effect |
|-------------|--------|
| `mapsharedmem rx, ry` | Map shared region # in rx into address space; return address in ry |

### Heap (Module 5)

| Instruction | Effect |
|-------------|--------|
| `alloc rx, ry` | Allocate rx bytes on heap; return address in ry (0 = failure) |
| `freememory rx` | Free heap allocation at address in rx |

---

## Module Breakdown

### Module 1 — CPU and Instruction Set
The virtual CPU has 14 integer registers, a sign flag, a zero flag, and a stack pointer. Each instruction is exactly 9 bytes (1 opcode + 4 arg1 + 4 arg2). The fetch-decode-execute loop dispatches on a `switch` over the `Opcode` enum. Every instruction costs exactly one clock cycle.

### Module 2 — Memory Paging
Every memory access is translated through a page table. A 32-bit virtual address is split into a 24-bit page number (upper bits) and an 8-bit offset (lower bits). `ReadInt32` and `WriteInt32` operate byte-by-byte to correctly handle integers that straddle page boundaries.

### Module 3 — Process Management
Each process has a PCB storing its full register state, page table, priority (1–32), time quantum (default 10), and lifecycle counters. The scheduler uses a priority-descending LINQ query to select the next process. Context switches save and restore all 14 registers, both flags, and the page table.

### Module 4 — Locks, Events, Shared Memory
Ten OS-provided locks (integer array, 0=free or owning PID) and ten events (boolean array). A process that fails to acquire a held lock does not advance its IP — it will retry the same `AcquireLock` instruction when rescheduled. Priority inversion is handled by temporarily boosting the lock holder's priority to match the highest-priority waiter, tracked precisely via `PCB.WaitingOnLockId`.

### Module 5 — Heap Allocation
First-fit allocator within a 512-byte per-process heap region. Freed blocks are tracked in a `List<HeapBlock>` and reused before the heap pointer is bumped. Allocations that would exceed the heap boundary return 0 (failure).

### Module 6 — Virtual Memory
Virtual page table has 512 entries; physical RAM holds 256. Pages beyond physical RAM start as `IsValid = false`. Access to an invalid page triggers `HandlePageFault`, which selects the LRU valid page for eviction. Dirty pages are saved to a simulated disk (`Dictionary<int, byte[]>`) before eviction; clean pages are discarded without saving. The page fault count is routed to the active PCB via a delegate set in `CPU.LoadState`.

---

## Test Programs

The following test files are provided to verify each module:

```bash
dotnet run -- 65536 mod1_test.txt                       # CPU and instructions
dotnet run -- 65536 mod2_test.txt                       # Paging
dotnet run -- 65536 mod3a_test.txt mod3b_test.txt       # Process management
dotnet run -- 65536 mod4a_test.txt mod4b_test.txt       # Locks and events
dotnet run -- 65536 mod5_test.txt                       # Heap allocation
dotnet run -- 65536 mod6_test.txt                       # Virtual memory
```

### Module 1 Expected Output
```
[P1] 15        (addr)
[P1] 16        (incr)
[P1] 8         (addi)
[P1] 999       (movrm + movmr roundtrip)
[P1] 999       (movmm roundtrip)
A              (printcr)
[P1] 10        (jlt skips correctly)
[P1] 20        (jgt skips correctly)
[P1] 30        (je skips correctly)
PASSED
[P1] 15        (loop exit)
[P1] 42        (stack push/pop)
[P1] 55        (subroutine - inside)
[P1] 55        (subroutine - after return)
```

### Module 3 Expected Behavior
- Process A (priority 20) runs exclusively until it sleeps
- While A sleeps, Process B (priority 10) runs and idle process fills gaps
- A wakes and completes; both exit reports print

### Module 4 Expected Behavior
- Critical sections from two processes never interleave
- 999 only prints after the signaling process has run

### Module 6 Expected Output
```
[MMU] PAGE FAULT! Swapping Virtual Page 300 into RAM...
[P1] 0
[P1] 54321
```
Exit report shows `Page Faults: 1` or more.

---

## Project Structure

```
MID/
├── MID.csproj
├── idle.txt              ← auto-generated if missing
├── Program.cs            ← entry point, boot sequence
├── CPU.cs                ← fetch-decode-execute, all opcodes, context switch
├── Opcodes.cs            ← enum of all instructions
├── MemoryManager.cs      ← MMU, paging, page fault handler, LRU eviction
├── VirtualPage.cs        ← page table entry (IsValid, IsDirty, LastAccessed)
├── Loader.cs             ← assembly parser, memory layout, register init
├── Scheduler.cs          ← OS kernel, scheduling loop, priority inversion
├── PCB.cs                ← Process Control Block
├── ProcessState.cs       ← process lifecycle enum
└── test/
    ├── mod1_test.txt
    ├── mod2_test.txt
    ├── mod3a_test.txt
    ├── mod3b_test.txt
    ├── mod4a_test.txt
    ├── mod4b_test.txt
    ├── mod5_test.txt
    └── mod6_test.txt
```

---

## Key Design Decisions

**Fixed 9-byte instruction format** — Every instruction occupies exactly 9 bytes regardless of how many arguments it uses. This makes the fetch loop trivial and eliminates variable-length parsing at runtime.

**Relative jumps from next instruction** — Jump offsets are relative to the instruction after the jump, matching the spec's convention. The formula is `new IP = (current IP + 9) + offset`.

**Byte-by-byte ReadInt32/WriteInt32** — Four-byte integers are assembled from individual byte reads rather than block copies. This correctly handles integers that straddle page boundaries without any special-case logic.

**OnPageFault delegate** — The MemoryManager notifies the CPU of page faults via an `Action` delegate set in `LoadState`. This avoids a circular dependency while still routing fault counts to the correct PCB.

**WaitingOnLockId in PCB** — When a process blocks on a lock, the specific lock ID is recorded in the PCB. The scheduler uses this for precise priority inversion rather than approximating which lock is involved.

**jumped flag in RunProcess** — Instructions that change control flow (jumps, calls, blocks) set a `jumped` boolean that suppresses the normal `IP += 9` advance at the end of the loop iteration. This prevents double-advancing the instruction pointer.

---

## License

This project was built as an academic assignment. Feel free to reference the design and implementation for educational purposes.
