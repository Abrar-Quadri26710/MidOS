using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MID
{
    //I had a [Cite Start} here
    //Refer to module 1 for opcode meaninsg 
    public enum Opcode : byte
    {
        INCR = 1,
        ADDI,
        ADDR,
        PUSHR,
        PUSHI,
        MOVI,
        MOVR,
        MOVMR,
        MOVRM,
        MOVMM,
        PRINTR,
        PRINTM,
        PRINTCR,
        PRINTCM,
        JMP,
        JMPI,
        JMPA,
        CMPI,
        CMPR,
        JLT,
        JLTI,
        JLTA,
        JGT,
        JGTI,
        JGTA,
        JE,
        JEI,
        JEA,
        CALL,
        CALLM,
        RET,
        EXIT,
        POPR,
        POPM,
        SLEEP,
        INPUT,
        INPUTC,
        SETPRIORITY,
        SETPRIORITYL,
    }

}
