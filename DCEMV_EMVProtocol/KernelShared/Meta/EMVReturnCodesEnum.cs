/*
*************************************************************************
DC EMV
Open Source EMV
Copyright (C) 2018  Vicente Da Silva

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see http://www.gnu.org/licenses/
*************************************************************************
*/
using DCEMV.Shared;
using DCEMV.ISO7816Protocol;
using System.Collections.Generic;

namespace DCEMV.EMVProtocol.Kernels
{
    public static class EMVReturnCodesEnum
    {
        private static List<EnumBase> data = new List<EnumBase>();

        static EMVReturnCodesEnum()
        {
            data.Add(new AdpuReturnResult(0x61, new SW2Range(0x00, 0xFF), ReturnType.Info, "Command successfully executed; 'XX' bytes of data are available and can be requested using GET RESPONSE. "));
            data.Add(new AdpuReturnResult(0x62, new SW2Range(0x00), ReturnType.Warning, "No information given (NV-Ram not changed) "));
            data.Add(new AdpuReturnResult(0x62, new SW2Range(0x01), ReturnType.Warning, "NV-Ram not changed 1. "));
            data.Add(new AdpuReturnResult(0x62, new SW2Range(0x81), ReturnType.Warning, "Part of returned data may be corrupted "));
            data.Add(new AdpuReturnResult(0x62, new SW2Range(0x82), ReturnType.Warning, "End of file/record reached before reading Le bytes "));
            data.Add(new AdpuReturnResult(0x62, new SW2Range(0x83), ReturnType.Warning, "Selected file invalidated "));
            data.Add(new AdpuReturnResult(0x62, new SW2Range(0x84), ReturnType.Warning, "Selected file is not valid. FCI not formated according to ISO "));
            data.Add(new AdpuReturnResult(0x62, new SW2Range(0x85), ReturnType.Warning, "No input data available from a sensor on the card. No Purse Engine enslaved for R3bc "));
            data.Add(new AdpuReturnResult(0x62, new SW2Range(0xA2), ReturnType.Warning, "Wrong R-MAC "));
            data.Add(new AdpuReturnResult(0x62, new SW2Range(0xA4), ReturnType.Warning, "Card locked (during reset( )) "));
            data.Add(new AdpuReturnResult(0x62, new SW2Range(0xC0, 0xCF), ReturnType.Warning, "Counter with value x (command dependent) "));
            data.Add(new AdpuReturnResult(0x62, new SW2Range(0xF1), ReturnType.Warning, "Wrong C-MAC "));
            data.Add(new AdpuReturnResult(0x62, new SW2Range(0xF3), ReturnType.Warning, "Internal reset "));
            data.Add(new AdpuReturnResult(0x62, new SW2Range(0xF5), ReturnType.Warning, "Default agent locked "));
            data.Add(new AdpuReturnResult(0x62, new SW2Range(0xF7), ReturnType.Warning, "Cardholder locked "));
            data.Add(new AdpuReturnResult(0x62, new SW2Range(0xF8), ReturnType.Warning, "Basement is current agent "));
            data.Add(new AdpuReturnResult(0x62, new SW2Range(0xF9), ReturnType.Warning, "CALC Key Set not unblocked "));
            data.Add(new AdpuReturnResult(0x63, new SW2Range(0x00), ReturnType.Warning, "No information given (NV-Ram changed) "));
            data.Add(new AdpuReturnResult(0x63, new SW2Range(0x81), ReturnType.Warning, "File filled up by the last write. Loading/updating is not allowed. "));
            data.Add(new AdpuReturnResult(0x63, new SW2Range(0x82), ReturnType.Warning, "Card key not supported. "));
            data.Add(new AdpuReturnResult(0x63, new SW2Range(0x83), ReturnType.Warning, "Reader key not supported. "));
            data.Add(new AdpuReturnResult(0x63, new SW2Range(0x84), ReturnType.Warning, "Plaintext transmission not supported. "));
            data.Add(new AdpuReturnResult(0x63, new SW2Range(0x85), ReturnType.Warning, "Secured transmission not supported. "));
            data.Add(new AdpuReturnResult(0x63, new SW2Range(0x86), ReturnType.Warning, "Volatile memory is not available. "));
            data.Add(new AdpuReturnResult(0x63, new SW2Range(0x87), ReturnType.Warning, "Non-volatile memory is not available. "));
            data.Add(new AdpuReturnResult(0x63, new SW2Range(0x88), ReturnType.Warning, "Key number not valid. "));
            data.Add(new AdpuReturnResult(0x63, new SW2Range(0x89), ReturnType.Warning, "Key length is not correct. "));
            data.Add(new AdpuReturnResult(0x63, new SW2Range(0xC0), ReturnType.Warning, "Verify fail, no try left. "));
            data.Add(new AdpuReturnResult(0x63, new SW2Range(0xC1), ReturnType.Warning, "Verify fail, 1 try left. "));
            data.Add(new AdpuReturnResult(0x63, new SW2Range(0xC2), ReturnType.Warning, "Verify fail, 2 tries left. "));
            data.Add(new AdpuReturnResult(0x63, new SW2Range(0xC3), ReturnType.Warning, "Verify fail, 3 tries left. "));
            data.Add(new AdpuReturnResult(0x63, new SW2Range(0xC4, 0xCF), ReturnType.Warning, "The counter has reached the value 'x' (0 = x = 15) (command dependent). "));
            data.Add(new AdpuReturnResult(0x63, new SW2Range(0xF1), ReturnType.Warning, "More data expected. "));
            data.Add(new AdpuReturnResult(0x63, new SW2Range(0xF2), ReturnType.Warning, "More data expected and proactive command pending. "));
            data.Add(new AdpuReturnResult(0x64, new SW2Range(0x00), ReturnType.Error, "No information given (NV-Ram not changed) "));
            data.Add(new AdpuReturnResult(0x64, new SW2Range(0x01), ReturnType.Error, "Command timeout. Immediate response required by the card. "));
            data.Add(new AdpuReturnResult(0x65, new SW2Range(0x00), ReturnType.Error, "No information given "));
            data.Add(new AdpuReturnResult(0x65, new SW2Range(0x01), ReturnType.Error, "Write error. Memory failure. There have been problems in writing or reading the EEPROM. Other hardware problems may public static EMVReturnCode _also bring this error. "));
            data.Add(new AdpuReturnResult(0x65, new SW2Range(0x81), ReturnType.Error, "Memory failure "));
            data.Add(new AdpuReturnResult(0x66, new SW2Range(0x00), ReturnType.Security, "Error while receiving (timeout) "));
            data.Add(new AdpuReturnResult(0x66, new SW2Range(0x01), ReturnType.Security, "Error while receiving (character parity error) "));
            data.Add(new AdpuReturnResult(0x66, new SW2Range(0x02), ReturnType.Security, "Wrong checksum "));
            data.Add(new AdpuReturnResult(0x66, new SW2Range(0x03), ReturnType.Security, "The current DF file without FCI "));
            data.Add(new AdpuReturnResult(0x66, new SW2Range(0x04), ReturnType.Security, "No SF or KF under the current DF "));
            data.Add(new AdpuReturnResult(0x66, new SW2Range(0x69), ReturnType.Security, "Incorrect Encryption/Decryption Padding "));
            data.Add(new AdpuReturnResult(0x67, new SW2Range(0x00), ReturnType.Error, "Wrong length "));
            data.Add(new AdpuReturnResult(0x67, new SW2Range(0x01, 0xFF), ReturnType.Error, "Length incorrect (procedure)(ISO 7816-3) "));
            data.Add(new AdpuReturnResult(0x68, new SW2Range(0x00), ReturnType.Error, "No information given (The request function is not supported by the card) "));
            data.Add(new AdpuReturnResult(0x68, new SW2Range(0x81), ReturnType.Error, "Logical channel not supported "));
            data.Add(new AdpuReturnResult(0x68, new SW2Range(0x82), ReturnType.Error, "Secure messaging not supported "));
            data.Add(new AdpuReturnResult(0x68, new SW2Range(0x83), ReturnType.Error, "Last command of the chain expected "));
            data.Add(new AdpuReturnResult(0x68, new SW2Range(0x84), ReturnType.Error, "Command chaining not supported "));
            data.Add(new AdpuReturnResult(0x69, new SW2Range(0x00), ReturnType.Error, "No information given (Command not allowed) "));
            data.Add(new AdpuReturnResult(0x69, new SW2Range(0x01), ReturnType.Error, "Command not accepted (inactive state) "));
            data.Add(new AdpuReturnResult(0x69, new SW2Range(0x81), ReturnType.Error, "Command incompatible with file structure "));
            data.Add(new AdpuReturnResult(0x69, new SW2Range(0x82), ReturnType.Error, "Security condition not satisfied. "));
            data.Add(new AdpuReturnResult(0x69, new SW2Range(0x83), ReturnType.Error, "Authentication method blocked "));
            data.Add(new AdpuReturnResult(0x69, new SW2Range(0x84), ReturnType.Error, "Referenced data reversibly blocked (invalidated) "));
            data.Add(new AdpuReturnResult(0x69, new SW2Range(0x85), ReturnType.Error, "Conditions of use not satisfied. "));
            data.Add(new AdpuReturnResult(0x69, new SW2Range(0x86), ReturnType.Error, "Command not allowed (no current EF) "));
            data.Add(new AdpuReturnResult(0x69, new SW2Range(0x87), ReturnType.Error, "Expected secure messaging (SM) object missing "));
            data.Add(new AdpuReturnResult(0x69, new SW2Range(0x88), ReturnType.Error, "Incorrect secure messaging (SM) data object "));
            data.Add(new AdpuReturnResult(0x69, new SW2Range(0x8D), ReturnType.Error, "Reserved"));
            data.Add(new AdpuReturnResult(0x69, new SW2Range(0x96), ReturnType.Error, "Data must be updated again "));
            data.Add(new AdpuReturnResult(0x69, new SW2Range(0xE1), ReturnType.Error, "POL1 of the currently Enabled Profile prevents this action. "));
            data.Add(new AdpuReturnResult(0x69, new SW2Range(0xF0), ReturnType.Error, "Permission Denied "));
            data.Add(new AdpuReturnResult(0x69, new SW2Range(0xF1), ReturnType.Error, "Permission Denied - Missing Privilege "));
            data.Add(new AdpuReturnResult(0x6A, new SW2Range(0x00), ReturnType.Error, "No information given (Bytes P1 and/or P2 are incorrect) "));
            data.Add(new AdpuReturnResult(0x6A, new SW2Range(0x80), ReturnType.Error, "The parameters in the data field are incorrect. "));
            data.Add(new AdpuReturnResult(0x6A, new SW2Range(0x81), ReturnType.Error, "Function not supported "));
            data.Add(new AdpuReturnResult(0x6A, new SW2Range(0x82), ReturnType.Error, "File not found "));
            data.Add(new AdpuReturnResult(0x6A, new SW2Range(0x83), ReturnType.Error, "Record not found "));
            data.Add(new AdpuReturnResult(0x6A, new SW2Range(0x84), ReturnType.Error, "There is insufficient memory space in record or file "));
            data.Add(new AdpuReturnResult(0x6A, new SW2Range(0x85), ReturnType.Error, "Lc inconsistent with TLV structure "));
            data.Add(new AdpuReturnResult(0x6A, new SW2Range(0x86), ReturnType.Error, "Incorrect P1 or P2 parameter. "));
            data.Add(new AdpuReturnResult(0x6A, new SW2Range(0x87), ReturnType.Error, "Lc inconsistent with P1-P2 "));
            data.Add(new AdpuReturnResult(0x6A, new SW2Range(0x88), ReturnType.Error, "Referenced data not found "));
            data.Add(new AdpuReturnResult(0x6A, new SW2Range(0x89), ReturnType.Error, "File already exists "));
            data.Add(new AdpuReturnResult(0x6A, new SW2Range(0x8A), ReturnType.Error, "DF name already exists. "));
            data.Add(new AdpuReturnResult(0x6A, new SW2Range(0xF0), ReturnType.Error, "Wrong parameter value "));
            data.Add(new AdpuReturnResult(0x6B, new SW2Range(0x00), ReturnType.Error, "Wrong parameter(s) P1-P2 "));
            data.Add(new AdpuReturnResult(0x6B, new SW2Range(0x01, 0xFF), ReturnType.Error, "Reference incorrect (procedure byte), (ISO 7816-3) "));
            data.Add(new AdpuReturnResult(0x6C, new SW2Range(0x00), ReturnType.Error, "Incorrect P3 length. "));
            data.Add(new AdpuReturnResult(0x6C, new SW2Range(0x01, 0xFF), ReturnType.Error, "Bad length value in Le; 'xx' is the correct exact Le "));
            data.Add(new AdpuReturnResult(0x6D, new SW2Range(0x00), ReturnType.Error, "Instruction code not supported or invalid "));
            data.Add(new AdpuReturnResult(0x6D, new SW2Range(0x01, 0xFF), ReturnType.Error, "Instruction code not programmed or invalid (procedure byte), (ISO 7816-3) "));
            data.Add(new AdpuReturnResult(0x6E, new SW2Range(0x00), ReturnType.Error, "Class not supported "));
            data.Add(new AdpuReturnResult(0x6E, new SW2Range(0x01, 0xFF), ReturnType.Error, "Instruction class not supported (procedure byte), (ISO 7816-3) "));
            data.Add(new AdpuReturnResult(0x6F, new SW2Range(0x00), ReturnType.Error, "Command aborted - more exact diagnosis not possible (e.g., operating system error). "));
            data.Add(new AdpuReturnResult(0x6F, new SW2Range(0xFF), ReturnType.Error, "Card dead (overuse, …) "));
            data.Add(new AdpuReturnResult(0x6F, new SW2Range(0x01, 0xEF), ReturnType.Error, "No precise diagnosis (procedure byte), (ISO 7816-3) "));
            data.Add(new AdpuReturnResult(0x90, new SW2Range(0x00), ReturnType.Info, "Command successfully executed (OK). "));
            data.Add(new AdpuReturnResult(0x90, new SW2Range(0x04), ReturnType.Warning, "PIN not succesfully verified, 3 or more PIN tries left "));
            data.Add(new AdpuReturnResult(0x90, new SW2Range(0x08), ReturnType.Error, "Key/file not found "));
            data.Add(new AdpuReturnResult(0x90, new SW2Range(0x80), ReturnType.Warning, "Unblock Try Counter has reached zero "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0x00), ReturnType.Error, "OK "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0x01), ReturnType.Error, "States.activity, States.lock Status or States.lockable has wrong value "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0x02), ReturnType.Error, "Transaction number reached its limit "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0x0C), ReturnType.Error, "No changes "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0x0E), ReturnType.Error, "Insufficient NV-Memory to complete command "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0x1C), ReturnType.Error, "Command code not supported "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0x1E), ReturnType.Error, "CRC or MAC does not match data "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0x40), ReturnType.Error, "Invalid key number specified "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0x7E), ReturnType.Error, "Length of command string invalid "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0x9D), ReturnType.Error, "Not allow the requested command "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0x9E), ReturnType.Error, "Value of the parameter invalid "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0xA0), ReturnType.Error, "Requested AID not present on PICC "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0xA1), ReturnType.Error, "Unrecoverable error within application "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0xAE), ReturnType.Error, "Authentication status does not allow the requested command "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0xAF), ReturnType.Error, "Additional data frame is expected to be sent "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0xBE), ReturnType.Error, "Out of boundary "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0xC1), ReturnType.Error, "Unrecoverable error within PICC "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0xCA), ReturnType.Error, "Previous Command was not fully completed "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0xCD), ReturnType.Error, "PICC was disabled by an unrecoverable error "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0xCE), ReturnType.Error, "Number of Applications limited to 28 "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0xDE), ReturnType.Error, "File or application already exists "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0xEE), ReturnType.Error, "Could not complete NV-write operation due to loss of power "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0xF0), ReturnType.Error, "Specified file number does not exist "));
            data.Add(new AdpuReturnResult(0x91, new SW2Range(0xF1), ReturnType.Error, "Unrecoverable error within file "));
            data.Add(new AdpuReturnResult(0x92, new SW2Range(0x01, 0x0F), ReturnType.Info, "Writing to EEPROM successful after 'x' attempts. "));
            data.Add(new AdpuReturnResult(0x92, new SW2Range(0x10), ReturnType.Error, "Insufficient memory. No more storage available. "));
            data.Add(new AdpuReturnResult(0x92, new SW2Range(0x40), ReturnType.Error, "Writing to EEPROM not successful. "));
            data.Add(new AdpuReturnResult(0x93, new SW2Range(0x01), ReturnType.Error, "Integrity error "));
            data.Add(new AdpuReturnResult(0x93, new SW2Range(0x02), ReturnType.Error, "Candidate S2 invalid "));
            data.Add(new AdpuReturnResult(0x93, new SW2Range(0x03), ReturnType.Error, "Application is permanently locked "));
            data.Add(new AdpuReturnResult(0x94, new SW2Range(0x00), ReturnType.Error, "No EF selected. "));
            data.Add(new AdpuReturnResult(0x94, new SW2Range(0x01), ReturnType.Error, "Candidate currency code does not match purse currency "));
            data.Add(new AdpuReturnResult(0x94, new SW2Range(0x02), ReturnType.Error, "Candidate amount too high OR Address range exceeded."));
            data.Add(new AdpuReturnResult(0x94, new SW2Range(0x03), ReturnType.Error, "Candidate amount too low "));
            data.Add(new AdpuReturnResult(0x94, new SW2Range(0x04), ReturnType.Error, "FID not found, record not found or comparison pattern not found. "));
            data.Add(new AdpuReturnResult(0x94, new SW2Range(0x05), ReturnType.Error, "Problems in the data field "));
            data.Add(new AdpuReturnResult(0x94, new SW2Range(0x06), ReturnType.Error, "Required MAC unavailable "));
            data.Add(new AdpuReturnResult(0x94, new SW2Range(0x07), ReturnType.Error, "Bad currency : purse engine has no slot with R3bc currency "));
            data.Add(new AdpuReturnResult(0x94, new SW2Range(0x08), ReturnType.Error, "R3bc currency not supported in purse engine OR Selected file type does not match command."));
            data.Add(new AdpuReturnResult(0x95, new SW2Range(0x80), ReturnType.Error, "Bad sequence "));
            data.Add(new AdpuReturnResult(0x96, new SW2Range(0x81), ReturnType.Error, "Slave not found "));
            data.Add(new AdpuReturnResult(0x97, new SW2Range(0x00), ReturnType.Error, "PIN blocked and Unblock Try Counter is 1 or 2 "));
            data.Add(new AdpuReturnResult(0x97, new SW2Range(0x02), ReturnType.Error, "Main keys are blocked "));
            data.Add(new AdpuReturnResult(0x97, new SW2Range(0x04), ReturnType.Error, "PIN not succesfully verified, 3 or more PIN tries left "));
            data.Add(new AdpuReturnResult(0x97, new SW2Range(0x84), ReturnType.Error, "Base key "));
            data.Add(new AdpuReturnResult(0x97, new SW2Range(0x85), ReturnType.Error, "Limit exceeded - C-MAC key "));
            data.Add(new AdpuReturnResult(0x97, new SW2Range(0x86), ReturnType.Error, "SM error - Limit exceeded - R-MAC key "));
            data.Add(new AdpuReturnResult(0x97, new SW2Range(0x87), ReturnType.Error, "Limit exceeded - sequence counter "));
            data.Add(new AdpuReturnResult(0x97, new SW2Range(0x88), ReturnType.Error, "Limit exceeded - R-MAC length "));
            data.Add(new AdpuReturnResult(0x97, new SW2Range(0x89), ReturnType.Error, "Service not available "));
            data.Add(new AdpuReturnResult(0x98, new SW2Range(0x02), ReturnType.Error, "No PIN defined. "));
            data.Add(new AdpuReturnResult(0x98, new SW2Range(0x04), ReturnType.Error, "Access conditions not satisfied, authentication failed. "));
            data.Add(new AdpuReturnResult(0x98, new SW2Range(0x35), ReturnType.Error, "ASK RANDOM or GIVE RANDOM not executed. "));
            data.Add(new AdpuReturnResult(0x98, new SW2Range(0x40), ReturnType.Error, "PIN verification not successful. "));
            data.Add(new AdpuReturnResult(0x98, new SW2Range(0x50), ReturnType.Error, "INCREASE or DECREASE could not be executed because a limit has been reached. "));
            data.Add(new AdpuReturnResult(0x98, new SW2Range(0x62), ReturnType.Error, "Authentication Error, application specific (incorrect MAC) "));
            data.Add(new AdpuReturnResult(0x99, new SW2Range(0x00), ReturnType.Error, "1 PIN try left "));
            data.Add(new AdpuReturnResult(0x99, new SW2Range(0x04), ReturnType.Error, "PIN not succesfully verified, 1 PIN try left "));
            data.Add(new AdpuReturnResult(0x99, new SW2Range(0x85), ReturnType.Error, "Wrong status - Cardholder lock "));
            data.Add(new AdpuReturnResult(0x99, new SW2Range(0x86), ReturnType.Error, "Missing privilege "));
            data.Add(new AdpuReturnResult(0x99, new SW2Range(0x87), ReturnType.Error, "PIN is not installed "));
            data.Add(new AdpuReturnResult(0x99, new SW2Range(0x88), ReturnType.Error, "Wrong status - R-MAC state "));
            data.Add(new AdpuReturnResult(0x9A, new SW2Range(0x00), ReturnType.Error, "2 PIN try left "));
            data.Add(new AdpuReturnResult(0x9A, new SW2Range(0x04), ReturnType.Error, "PIN not succesfully verified, 2 PIN try left "));
            data.Add(new AdpuReturnResult(0x9A, new SW2Range(0x71), ReturnType.Error, "Wrong parameter value - Double agent AID "));
            data.Add(new AdpuReturnResult(0x9A, new SW2Range(0x72), ReturnType.Error, "Wrong parameter value - Double agent Type "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x05), ReturnType.Error, "Incorrect certificate type "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x07), ReturnType.Error, "Incorrect session data size "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x08), ReturnType.Error, "Incorrect DIR file record size "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x09), ReturnType.Error, "Incorrect FCI record size "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x0A), ReturnType.Error, "Incorrect code size "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x10), ReturnType.Error, "Insufficient memory to load application "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x11), ReturnType.Error, "Invalid AID "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x12), ReturnType.Error, "Duplicate AID "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x13), ReturnType.Error, "Application previously loaded "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x14), ReturnType.Error, "Application history list full "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x15), ReturnType.Error, "Application not open "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x17), ReturnType.Error, "Invalid offset "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x18), ReturnType.Error, "Application already loaded "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x19), ReturnType.Error, "Invalid certificate "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x1A), ReturnType.Error, "Invalid signature "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x1B), ReturnType.Error, "Invalid KTU "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x1D), ReturnType.Error, "MSM controls not set "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x1E), ReturnType.Error, "Application signature does not exist "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x1F), ReturnType.Error, "KTU does not exist "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x20), ReturnType.Error, "Application not loaded "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x21), ReturnType.Error, "Invalid Open command data length "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x30), ReturnType.Error, "Check data parameter is incorrect (invalid start address) "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x31), ReturnType.Error, "Check data parameter is incorrect (invalid length) "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x32), ReturnType.Error, "Check data parameter is incorrect (illegal memory check area) "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x40), ReturnType.Error, "Invalid MSM Controls ciphertext "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x41), ReturnType.Error, "MSM controls already set "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x42), ReturnType.Error, "Set MSM Controls data length less than 2 bytes "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x43), ReturnType.Error, "Invalid MSM Controls data length "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x44), ReturnType.Error, "Excess MSM Controls ciphertext "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x45), ReturnType.Error, "Verification of MSM Controls data failed "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x50), ReturnType.Error, "Invalid MCD Issuer production ID "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x51), ReturnType.Error, "Invalid MCD Issuer ID "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x52), ReturnType.Error, "Invalid set MSM controls data date "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x53), ReturnType.Error, "Invalid MCD number "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x54), ReturnType.Error, "Reserved field error "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x55), ReturnType.Error, "Reserved field error "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x56), ReturnType.Error, "Reserved field error "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x57), ReturnType.Error, "Reserved field error "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x60), ReturnType.Error, "MAC verification failed "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x61), ReturnType.Error, "Maximum number of unblocks reached "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x62), ReturnType.Error, "Card was not blocked "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x63), ReturnType.Error, "Crypto functions not available "));
            data.Add(new AdpuReturnResult(0x9D, new SW2Range(0x64), ReturnType.Error, "No application loaded "));
            data.Add(new AdpuReturnResult(0x9E, new SW2Range(0x00), ReturnType.Error, "PIN not installed "));
            data.Add(new AdpuReturnResult(0x9E, new SW2Range(0x04), ReturnType.Error, "PIN not succesfully verified, PIN not installed "));
            data.Add(new AdpuReturnResult(0x9F, new SW2Range(0x00), ReturnType.Error, "PIN blocked and Unblock Try Counter is 3 "));
            data.Add(new AdpuReturnResult(0x9F, new SW2Range(0x04), ReturnType.Error, "PIN not succesfully verified, PIN blocked and Unblock Try Counter is 3 "));
            data.Add(new AdpuReturnResult(0x9F, new SW2Range(0x01, 0x03), ReturnType.Error, "Command successfully executed; 'xx' bytes of data are available and can be requested using GET RESPONSE. "));
            data.Add(new AdpuReturnResult(0x9F, new SW2Range(0x05, 0xFF), ReturnType.Error, "Command successfully executed; 'xx' bytes of data are available and can be requested using GET RESPONSE. "));
        }

        public static AdpuReturnResult FindReturnCode(byte sw1, byte sw2)
        {
            foreach(AdpuReturnResult rc in data)
            {
                if(rc.SW1 == sw1)
                {
                    if (rc.SW2.inRange(sw2))
                        return rc;
                }
            }
            return new AdpuReturnResult(sw1, new SW2Range(sw2), ReturnType.Error, "Unrecognized Return Values");
        }
    }
    public enum ISO7816ReturnCodes
    {
        OFFSET_CLA = 0,
        OFFSET_INS = 1,
        OFFSET_P1 = 2,
        OFFSET_P2 = 3,
        OFFSET_LC = 4,
        OFFSET_CDATA = 5,
        CLA_ISO7816 = 0,
        INS_ERASE_BINARY_0E = 14,
        INS_VERIFY_20 = 32,
        INS_CHANGE_CHV_24 = 36,
        INS_UNBLOCK_CHV_2C = 44,
        INS_EXTERNAL_AUTHENTICATE_82 = 130,
        INS_MUTUAL_AUTHENTICATE_82 = 130,
        INS_GET_CHALLENGE_84 = 132,
        INS_ASK_RANDOM = 132,
        INS_GIVE_RANDOM = 134,
        INS_INTERNAL_AUTHENTICATE = 136,
        INS_SEEK = 162,
        INS_SELECT = 164,
        INS_SELECT_FILE = 164,
        INS_CLOSE_APPLICATION = 172,
        INS_READ_BINARY = 176,
        INS_READ_BINARY2 = 177,
        INS_READ_RECORD = 178,
        INS_READ_RECORD2 = 179,
        INS_READ_RECORDS = 178,
        INS_GET_RESPONSE = 192,
        INS_ENVELOPE = 194,
        INS_GET_DATA = 202,
        INS_WRITE_BINARY = 208,
        INS_WRITE_RECORD = 210,
        INS_UPDATE_BINARY = 214,
        INS_LOAD_KEY_FILE = 216,
        INS_PUT_DATA = 218,
        INS_UPDATE_RECORD = 220,
        INS_CREATE_FILE = 224,
        INS_APPEND_RECORD = 226,
        INS_DELETE_FILE = 228,
        SW_BYTES_REMAINING_00 = 24832,
        SW_END_OF_FILE = 25218,
        SW_LESS_DATA_RESPONDED_THAN_REQUESTED = 25223,
        SW_WRONG_LENGTH = 26368,
        SW_SECURITY_STATUS_NOT_SATISFIED = 27010,
        SW_AUTHENTICATION_METHOD_BLOCKED = 27011,
        SW_DATA_INVALID = 27012,
        SW_CONDITIONS_OF_USE_NOT_SATISFIED = 27013,
        SW_COMMAND_NOT_ALLOWED = 27014,
        SW_EXPECTED_SM_DATA_OBJECTS_MISSING = 27015,
        SW_SM_DATA_OBJECTS_INCORRECT = 27016,
        SW_KEY_USAGE_ERROR = 27073,
        SW_WRONG_DATA = 27264,
        SW_FILEHEADER_INCONSISTENT = 27264,
        SW_FUNC_NOT_SUPPORTED = 27265,
        SW_FILE_NOT_FOUND = 27266,
        SW_RECORD_NOT_FOUND = 27267,
        SW_FILE_FULL = 27268,
        SW_OUT_OF_MEMORY = 27268,
        SW_INCORRECT_P1P2 = 27270,
        SW_KEY_NOT_FOUND = 27272,
        SW_WRONG_P1P2 = 27392,
        SW_CORRECT_LENGTH_00 = 27648,
        SW_INS_NOT_SUPPORTED = 27904,
        SW_CLA_NOT_SUPPORTED = 28160,
        SW_NO_PRECISE_DIAGNOSIS = 28416,
        SW_CARD_TERMINATED = 28671,
        SW_NO_ERROR = 36864,
    }

}
