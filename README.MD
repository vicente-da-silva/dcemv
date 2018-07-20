# DC EMV Open Source EMV Implementation
## V1.0

>[DC EMV Decoded Book available on Amazon Kindle](https://www.amazon.com/EMV-Decoded-transaction-switching-Kubernetes-ebook/dp/B07FCXBZDR/ref=sr_1_1?ie=UTF8&qid=1532082668&sr=8-1&keywords=DC+EMV) This will help you to quickly get up and running with the various applications and understand the various capabilities of the system.

Features:
* Contactless EMV Kernel 1,2,3 (EMV 2.6)
* Contact EMV Kernel (EMV 4.3)
* EMV Contact and EMV Contactless Terminal Payment Application
* EMV Payment Application UI Controls
* DC EMV JavaCard Applet (compatible with kernel 3)
* GlobalPlatform Personalization application
* Card Reader Drivers 
  * Android Builtin and ACS1255
  * Windows PC/SC
  * Raspberry Pi via WinIOT and OM5577
  * Virtual Card Reader to JavaCard Emulator
* Multiplatform (Xamarin) Closed Loop Payment Demo Client 
  * Online account creation, with email (SendGrid) verification and mobile phone (Twilio) verification.
  * Account Management (Forgot Password, Change Password, Update Phone Number)
  * Send Money (Sends money from the account associated with the app being used to the account associated with the DCEMV Card being tapped on the device)
  * Receive Money (Money is sent from the account of the DCEMV Card being tapped on the device to the account associated with the app being used)
  * Top-Account (An open loop credit or debit EMV card is tapped on the device and the account is topped up with the amount selected)
  * Sell Stuff (The user uses a POS style screen to create a shopping basket of items they are selling, and can then take a payment from the DCEMV Card, in the same way as Receive Money)
  * Manage Inventory (Inventory items can be added, removed, updated and associated with Inventory groups which can be added, removed, updated. Inventory groups are used on the POS screen as one of the ways for navigating the list of inventory items)
  * Card Admin (DCEMV Cards can be associated with the account associated with the app being used, cards can be disabled or removed from the account)
  * Account Transactions (An account statement)
  * Compatible with
    * Android 
    * Windows 
    * Windows IOT
    * iOS ready
* Demo Payment Processing Server
  * In Memory HSM
  * Closed Loop Payment System Restful API Implementation
* Authorization Drivers (used standalone or from Demo Payment Processing Server)
  * In Memory Simulated Payment Provider (ARQC validation / ARPC generation / Script generation (Pin Change))
  * SPDH Driver

 # Features / Enhancements planned are as follows
1.	“Pre-certify” the kernels by using certified testing tools such as the test tool from UL along with an Astrex or MasterCard and Visa simulator. If you have access to these tools and want to contribute to the project then please contact me.
2.	In order to use these kernels in a production environment where association cards are going to be accepted, the kernel needs to be certified on specific hardware. If there are any vendors making EMV level 1 certified hardware and who are interested in integrating this kernel onto their devices, please contact me.
3.	A magstripe kernel should be added to the code base, for legacy cards, if there is anyone who has had this kind of experience and wishes to contribute to the project, please contact me.
4.	Performance enhancements to speed up transaction execution time.
5.	Enhance the card applets to support both contact and contactless applications. With these a test tool can be created that implements the test pack/cases (for each card association) that a terminal vendor would need to certify their terminal against in order to get certification. This should not be that hard as the card state engine is a lot simpler than the kernels. Alternatively, a company like UL could enhance their test tool with an API that can be called by a test application to load cards rather than having to physically select the card to be loaded via the UI. 
6.	Develop an acquirer/issuer switch capable of routing transactions between systems using different protocols as well as approving / declining “on us” transactions. All the cryptographic requirements for this switch are already implemented in the Simulated Payment Provider module.
7.	Improve testability to the point where a full regression test can be done in completely automated way, using simulated cards and the switch mentioned above. This automated testing would be able to check card logs, terminal logs and switch logs and report on discrepancies across the entire transaction flow for all test cases in a matter of minutes.
8.	Certify physical cards for use with the applets. Any card manufacturers interested in having their cards certified for use with the DC EMV payment applets should contact me. I would like to be able to provide users of the DC EMV framework a list of cards certified to work with the applets. This will negate them having to find compatible cards themselves and will shorten their development cycle.
9.	Enhance the HSM module to do tokenization of cards. This simply requires adding an API, datastore and using the existing HSM crypto functions.
10.	Create an HCE version of the card applet, this will allow the DC EMV Demo App user to make/receive a payment without needing a physical card (users can tokenize their physical card, or create virtual cards within the Demo app on their NFC capable device) 
11.	Allow payments in the DC EMV Demo App using QR codes, this will negate the need for NFC hardware but then implies that both devices involved in the transaction must be capable of scanning/presenting a QR code.