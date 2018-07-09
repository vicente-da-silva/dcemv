# dcemv
Open Source EMV Implementation<br />
V1.0<br />
DC EMV Decoded Book available on Amazon Kindle<br />
<br />
Features:<br />
* Contactless EMV Kernel 1,2,3 (EMV 2.6)<br />
* Contact EMV Kernel (EMV 4.3)<br />
* EMV Contact and EMV Contactless Terminal Payment Application<br />
* EMV Payment Application UI Controls<br />
* DC EMV JavaCard Applet (compatible with kernel 3)<br />
* GlobalPlatform Personalization application<br />
* Card Reader Drivers <br />
  * Android Builtin and ACS1255<br />
  * Windows PC/SC<br />
  * Raspberry Pi via WinIOT and OM5577<br />
  * Virtual Card Reader to JavaCard Emulator<br />
* Multiplatform (Xamarin) Closed Loop Payment Demo Client <br />
  * Online account creation, with email (SendGrid) verification and mobile phone (Twilio) verification.<br />
  * Account Management (Forgot Password, Change Password, Update Phone Number)<br />
  * Send Money (Sends money from the account associated with the app being used to the account associated with the DCEMV Card being tapped on the device)<br />
  * Receive Money (Money is sent from the account of the DCEMV Card being tapped on the device to the account associated with the app being used)<br />
  * Top-Account (An open loop credit or debit EMV card is tapped on the device and the account is topped up with the amount selected)<br />
  * Sell Stuff (The user uses a POS style screen to create a shopping basket of items they are selling, and can then take a payment from the DCEMV Card, in the same way as Receive Money)<br />
  * Manage Inventory (Inventory items can be added, removed, updated and associated with Inventory groups which can be added, removed, updated. Inventory groups are used on the POS screen as one of the ways for navigating the list of inventory items)<br />
  * Card Admin (DCEMV Cards can be associated with the account associated with the app being used, cards can be disabled or removed from the account)<br />
  * Account Transactions (An account statement)
  * Compatible with
    * Android <br />
    * Windows <br />
    * Windows IOT<br />
    * iOS ready<br />
* Demo Payment Processing Server<br />
  * In Memory HSM<br />
  * Closed Loop Payment System Restful API Implementation<br />
* Authorization Drivers (used standalone or from Demo Payment Processing Server)<br />
  * In Memory Simulated Payment Provider (ARQC validation / ARPC generation / Script generation (Pin Change))<br />
  * SPDH Driver<br />
<br />
