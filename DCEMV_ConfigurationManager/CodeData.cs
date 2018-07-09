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
namespace DCEMV.ConfigurationManager
{
    public static class CodeData
    {
        #region Certs
        //https://www.eftlab.co.uk/index.php/site-map/knowledge-base/243-ca-public-keys
        public const string Certs = @"<?xml version = ""1.0"" encoding=""utf-8""?>
        <ArrayOfCAPublicKeyCertificate xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
          <CAPublicKeyCertificate>
            <HashAlgorithmIndicator>1</HashAlgorithmIndicator>
            <PublicKeyAlgorithmIndicator>1</PublicKeyAlgorithmIndicator>
            <Modulus>A90FCD55AA2D5D9963E35ED0F440177699832F49C6BAB15CDAE5794BE93F934D4462D5D12762E48C38BA83D8445DEAA74195A301A102B2F114EADA0D180EE5E7A5C73E0C4E11F67A43DDAB5D55683B1474CC0627F44B8D3088A492FFAADAD4F42422D0E7013536C3C49AD3D0FAE96459B0F6B1B6056538A3D6D44640F94467B108867DEC40FAAECD740C00E2B7A8852D</Modulus>
            <Exponent>03</Exponent>
            <ExpiryDate/>
            <Index>250</Index>
            <ActivationDate />
            <Checksum />
            <RID>A000000004</RID>
          </CAPublicKeyCertificate>
          <CAPublicKeyCertificate>
            <HashAlgorithmIndicator>1</HashAlgorithmIndicator>
            <PublicKeyAlgorithmIndicator>1</PublicKeyAlgorithmIndicator>
            <Modulus>A191CB87473F29349B5D60A88B3EAEE0973AA6F1A082F358D849FDDFF9C091F899EDA9792CAF09EF28F5D22404B88A2293EEBBC1949C43BEA4D60CFD879A1539544E09E0F09F60F065B2BF2A13ECC705F3D468B9D33AE77AD9D3F19CA40F23DCF5EB7C04DC8F69EBA565B1EBCB4686CD274785530FF6F6E9EE43AA43FDB02CE00DAEC15C7B8FD6A9B394BABA419D3F6DC85E16569BE8E76989688EFEA2DF22FF7D35C043338DEAA982A02B866DE5328519EBBCD6F03CDD686673847F84DB651AB86C28CF1462562C577B853564A290C8556D818531268D25CC98A4CC6A0BDFFFDA2DCCA3A94C998559E307FDDF915006D9A987B07DDAEB3B</Modulus>
            <Exponent>03</Exponent>
            <ExpiryDate/>
            <Index>239</Index>
            <ActivationDate />
            <Checksum />
            <RID>A000000004</RID>
          </CAPublicKeyCertificate>
          <CAPublicKeyCertificate>
            <HashAlgorithmIndicator>1</HashAlgorithmIndicator>
            <PublicKeyAlgorithmIndicator>1</PublicKeyAlgorithmIndicator>
            <Modulus>CBCBDB21CCE3A3369EC6FFAB4C26D8F8660FBC75DBA42115F9EDB4E3D654AAF2434CCEB57837449D0A0A59683D3A199F2FC36149951EBAA192D1336A043016BE72F443B1A4B636CF7A464F59E480B79C0A8A68684822278B0922C4800931DEC8C73F58B2DE8B2EBA4203AD621914738AB49EF5653A6873AC4216E8127D275DB9E4A9A848CDF85B5BD4719F19A33EF953</Modulus>
            <Exponent>03</Exponent>
            <ExpiryDate>1220</ExpiryDate>
            <Index>173</Index>
            <ActivationDate />
            <Checksum />
            <RID>A000000004</RID>
          </CAPublicKeyCertificate>
          <CAPublicKeyCertificate>
            <HashAlgorithmIndicator>1</HashAlgorithmIndicator>
            <PublicKeyAlgorithmIndicator>1</PublicKeyAlgorithmIndicator>
            <Modulus>A89F25A56FA6DA258C8CA8B40427D927B4A1EB4D7EA326BBB12F97DED70AE5E4480FC9C5E8A972177110A1CC318D06D2F8F5C4844AC5FA79A4DC470BB11ED635699C17081B90F1B984F12E92C1C529276D8AF8EC7F28492097D8CD5BECEA16FE4088F6CFAB4A1B42328A1B996F9278B0B7E3311CA5EF856C2F888474B83612A82E4E00D0CD4069A6783140433D50725F</Modulus>
            <Exponent>03</Exponent>
            <ExpiryDate>1217</ExpiryDate>
            <Index>7</Index>
            <ActivationDate />
            <Checksum>B4BC56CC4E88324932CBC643D6898F6FE593B172</Checksum>
            <RID>A000000003</RID>
          </CAPublicKeyCertificate>
          <CAPublicKeyCertificate>
            <HashAlgorithmIndicator>1</HashAlgorithmIndicator>
            <PublicKeyAlgorithmIndicator>16</PublicKeyAlgorithmIndicator>
            <Modulus>D9FD6ED75D51D0E30664BD157023EAA1FFA871E4DA65672B863D255E81E137A51DE4F72BCC9E44ACE12127F87E263D3AF9DD9CF35CA4A7B01E907000BA85D24954C2FCA3074825DDD4C0C8F186CB020F683E02F2DEAD3969133F06F7845166ACEB57CA0FC2603445469811D293BFEFBAFAB57631B3DD91E796BF850A25012F1AE38F05AA5C4D6D03B1DC2E568612785938BBC9B3CD3A910C1DA55A5A9218ACE0F7A21287752682F15832A678D6E1ED0B</Modulus>
            <Exponent>03</Exponent>
            <ExpiryDate>1224</ExpiryDate>
            <Index>8</Index>
            <ActivationDate />
            <Checksum>20D213126955DE205ADC2FD2822BD22DE21CF9A8</Checksum>
            <RID>A000000003</RID>
          </CAPublicKeyCertificate>
          <CAPublicKeyCertificate>
            <HashAlgorithmIndicator>1</HashAlgorithmIndicator>
            <PublicKeyAlgorithmIndicator>1</PublicKeyAlgorithmIndicator>
            <Modulus>9D912248DE0A4E39C1A7DDE3F6D2588992C1A4095AFBD1824D1BA74847F2BC4926D2EFD904B4B54954CD189A54C5D1179654F8F9B0D2AB5F0357EB642FEDA95D3912C6576945FAB897E7062CAA44A4AA06B8FE6E3DBA18AF6AE3738E30429EE9BE03427C9D64F695FA8CAB4BFE376853EA34AD1D76BFCAD15908C077FFE6DC5521ECEF5D278A96E26F57359FFAEDA19434B937F1AD999DC5C41EB11935B44C18100E857F431A4A5A6BB65114F174C2D7B59FDF237D6BB1DD0916E644D709DED56481477C75D95CDD68254615F7740EC07F330AC5D67BCD75BF23D28A140826C026DBDE971A37CD3EF9B8DF644AC385010501EFC6509D7A41</Modulus>
            <Exponent>03</Exponent>
            <ExpiryDate>1225</ExpiryDate>
            <Index>9</Index>
            <ActivationDate />
            <Checksum>1FF80A40173F52D7D27E0F26A146A1C8CCB29046</Checksum>
            <RID>A000000003</RID>
          </CAPublicKeyCertificate>
          <CAPublicKeyCertificate>
            <HashAlgorithmIndicator>1</HashAlgorithmIndicator>
            <PublicKeyAlgorithmIndicator>1</PublicKeyAlgorithmIndicator>
            <Modulus>996AF56F569187D09293C14810450ED8EE3357397B18A2458EFAA92DA3B6DF6514EC060195318FD43BE9B8F0CC669E3F844057CBDDF8BDA191BB64473BC8DC9A730DB8F6B4EDE3924186FFD9B8C7735789C23A36BA0B8AF65372EB57EA5D89E7D14E9C7B6B557460F10885DA16AC923F15AF3758F0F03EBD3C5C2C949CBA306DB44E6A2C076C5F67E281D7EF56785DC4D75945E491F01918800A9E2DC66F60080566CE0DAF8D17EAD46AD8E30A247C9F</Modulus>
            <Exponent>03</Exponent>
            <ExpiryDate>1250</ExpiryDate>
            <Index>146</Index>
            <ActivationDate />
            <Checksum>429C954A3859CEF91295F663C963E582ED6EB253</Checksum>
            <RID>A000000003</RID>
          </CAPublicKeyCertificate>
          <CAPublicKeyCertificate>
            <HashAlgorithmIndicator>1</HashAlgorithmIndicator>
            <PublicKeyAlgorithmIndicator>1</PublicKeyAlgorithmIndicator>
            <Modulus>ACD2B12302EE644F3F835ABD1FC7A6F62CCE48FFEC622AA8EF062BEF6FB8BA8BC68BBF6AB5870EED579BC3973E121303D34841A796D6DCBC41DBF9E52C4609795C0CCF7EE86FA1D5CB041071ED2C51D2202F63F1156C58A92D38BC60BDF424E1776E2BC9648078A03B36FB554375FC53D57C73F5160EA59F3AFC5398EC7B67758D65C9BFF7828B6B82D4BE124A416AB7301914311EA462C19F771F31B3B57336000DFF732D3B83DE07052D730354D297BEC72871DCCF0E193F171ABA27EE464C6A97690943D59BDABB2A27EB71CEEBDAFA1176046478FD62FEC452D5CA393296530AA3F41927ADFE434A2DF2AE3054F8840657A26E0FC617</Modulus>
            <Exponent>03</Exponent>
            <ExpiryDate>1250</ExpiryDate>
            <Index>148</Index>
            <ActivationDate />
            <Checksum>C4A3C43CCF87327D136B804160E47D43B60E6E0F</Checksum>
            <RID>A000000003</RID>
          </CAPublicKeyCertificate>
          <CAPublicKeyCertificate>
            <HashAlgorithmIndicator>1</HashAlgorithmIndicator>
            <PublicKeyAlgorithmIndicator>1</PublicKeyAlgorithmIndicator>
            <Modulus>BE9E1FA5E9A803852999C4AB432DB28600DCD9DAB76DFAAA47355A0FE37B1508AC6BF38860D3C6C2E5B12A3CAAF2A7005A7241EBAA7771112C74CF9A0634652FBCA0E5980C54A64761EA101A114E0F0B5572ADD57D010B7C9C887E104CA4EE1272DA66D997B9A90B5A6D624AB6C57E73C8F919000EB5F684898EF8C3DBEFB330C62660BED88EA78E909AFF05F6DA627B</Modulus>
            <Exponent>03</Exponent>
            <ExpiryDate>1250</ExpiryDate>
            <Index>149</Index>
            <ActivationDate />
            <Checksum>EE1511CEC71020A9B90443B37B1D5F6E703030F6</Checksum>
            <RID>A000000003</RID>
          </CAPublicKeyCertificate>
          <CAPublicKeyCertificate>
            <HashAlgorithmIndicator>1</HashAlgorithmIndicator>
            <PublicKeyAlgorithmIndicator>1</PublicKeyAlgorithmIndicator>
            <Modulus>98F0C770F23864C2E766DF02D1E833DFF4FFE92D696E1642F0A88C5694C6479D16DB1537BFE29E4FDC6E6E8AFD1B0EB7EA0124723C333179BF19E93F10658B2F776E829E87DAEDA9C94A8B3382199A350C077977C97AFF08FD11310AC950A72C3CA5002EF513FCCC286E646E3C5387535D509514B3B326E1234F9CB48C36DDD44B416D23654034A66F403BA511C5EFA3</Modulus>
            <Exponent>03</Exponent>
            <ExpiryDate>1250</ExpiryDate>
            <Index>243</Index>
            <ActivationDate />
            <Checksum>128EB33128E63E38C9A83A2B1A9349E178F82196</Checksum>
            <RID>A000000003</RID>
          </CAPublicKeyCertificate>
          <CAPublicKeyCertificate>
            <HashAlgorithmIndicator>1</HashAlgorithmIndicator>
            <PublicKeyAlgorithmIndicator>1</PublicKeyAlgorithmIndicator>
            <Modulus>A6DA428387A502D7DDFB7A74D3F412BE762627197B25435B7A81716A700157DDD06F7CC99D6CA28C2470527E2C03616B9C59217357C2674F583B3BA5C7DCF2838692D023E3562420B4615C439CA97C44DC9A249CFCE7B3BFB22F68228C3AF13329AA4A613CF8DD853502373D62E49AB256D2BC17120E54AEDCED6D96A4287ACC5C04677D4A5A320DB8BEE2F775E5FEC5</Modulus>
            <Exponent>03</Exponent>
            <ExpiryDate>1217</ExpiryDate>
            <Index>4</Index>
            <ActivationDate />
            <Checksum>381A035DA58B482EE2AF75F4C3F2CA469BA4AA6C</Checksum>
            <RID>A000000004</RID>
          </CAPublicKeyCertificate>
          <CAPublicKeyCertificate>
            <HashAlgorithmIndicator>1</HashAlgorithmIndicator>
            <PublicKeyAlgorithmIndicator>1</PublicKeyAlgorithmIndicator>
            <Modulus>B8048ABC30C90D976336543E3FD7091C8FE4800DF820ED55E7E94813ED00555B573FECA3D84AF6131A651D66CFF4284FB13B635EDD0EE40176D8BF04B7FD1C7BACF9AC7327DFAA8AA72D10DB3B8E70B2DDD811CB4196525EA386ACC33C0D9D4575916469C4E4F53E8E1C912CC618CB22DDE7C3568E90022E6BBA770202E4522A2DD623D180E215BD1D1507FE3DC90CA310D27B3EFCCD8F83DE3052CAD1E48938C68D095AAC91B5F37E28BB49EC7ED597</Modulus>
            <Exponent>03</Exponent>
            <ExpiryDate>1221</ExpiryDate>
            <Index>5</Index>
            <ActivationDate />
            <Checksum>EBFA0D5D06D8CE702DA3EAE890701D45E274C845</Checksum>
            <RID>A000000004</RID>
          </CAPublicKeyCertificate>
          <CAPublicKeyCertificate>
            <HashAlgorithmIndicator>1</HashAlgorithmIndicator>
            <PublicKeyAlgorithmIndicator>1</PublicKeyAlgorithmIndicator>
            <Modulus>CB26FC830B43785B2BCE37C81ED334622F9622F4C89AAE641046B2353433883F307FB7C974162DA72F7A4EC75D9D657336865B8D3023D3D645667625C9A07A6B7A137CF0C64198AE38FC238006FB2603F41F4F3BB9DA1347270F2F5D8C606E420958C5F7D50A71DE30142F70DE468889B5E3A08695B938A50FC980393A9CBCE44AD2D64F630BB33AD3F5F5FD495D31F37818C1D94071342E07F1BEC2194F6035BA5DED3936500EB82DFDA6E8AFB655B1EF3D0D7EBF86B66DD9F29F6B1D324FE8B26CE38AB2013DD13F611E7A594D675C4432350EA244CC34F3873CBA06592987A1D7E852ADC22EF5A2EE28132031E48F74037E3B34AB747F</Modulus>
            <Exponent>03</Exponent>
            <ExpiryDate>1221</ExpiryDate>
            <Index>6</Index>
            <ActivationDate />
            <Checksum>F910A1504D5FFB793D94F3B500765E1ABCAD72D9</Checksum>
            <RID>A000000004</RID>
          </CAPublicKeyCertificate>
          <CAPublicKeyCertificate>
            <HashAlgorithmIndicator>1</HashAlgorithmIndicator>
            <PublicKeyAlgorithmIndicator>1</PublicKeyAlgorithmIndicator>
            <Modulus>A0DCF4BDE19C3546B4B6F0414D174DDE294AABBB828C5A834D73AAE27C99B0B053A90278007239B6459FF0BBCD7B4B9C6C50AC02CE91368DA1BD21AAEADBC65347337D89B68F5C99A09D05BE02DD1F8C5BA20E2F13FB2A27C41D3F85CAD5CF6668E75851EC66EDBF98851FD4E42C44C1D59F5984703B27D5B9F21B8FA0D93279FBBF69E090642909C9EA27F898959541AA6757F5F624104F6E1D3A9532F2A6E51515AEAD1B43B3D7835088A2FAFA7BE7</Modulus>
            <Exponent>03</Exponent>
            <ExpiryDate>1250</ExpiryDate>
            <Index>241</Index>
            <ActivationDate />
            <Checksum>D8E68DA167AB5A85D8C3D55ECB9B0517A1A5B4BB</Checksum>
            <RID>A000000004</RID>
          </CAPublicKeyCertificate>
          <CAPublicKeyCertificate>
            <HashAlgorithmIndicator>1</HashAlgorithmIndicator>
            <PublicKeyAlgorithmIndicator>1</PublicKeyAlgorithmIndicator>
            <Modulus>98F0C770F23864C2E766DF02D1E833DFF4FFE92D696E1642F0A88C5694C6479D16DB1537BFE29E4FDC6E6E8AFD1B0EB7EA0124723C333179BF19E93F10658B2F776E829E87DAEDA9C94A8B3382199A350C077977C97AFF08FD11310AC950A72C3CA5002EF513FCCC286E646E3C5387535D509514B3B326E1234F9CB48C36DDD44B416D23654034A66F403BA511C5EFA3</Modulus>
            <Exponent>03</Exponent>
            <ExpiryDate>1250</ExpiryDate>
            <Index>243</Index>
            <ActivationDate />
            <Checksum>A69AC7603DAF566E972DEDC2CB433E07E8B01A9A</Checksum>
            <RID>A000000004</RID>
          </CAPublicKeyCertificate>
          <CAPublicKeyCertificate>
            <HashAlgorithmIndicator>1</HashAlgorithmIndicator>
            <PublicKeyAlgorithmIndicator>1</PublicKeyAlgorithmIndicator>
            <Modulus>A6E6FB72179506F860CCCA8C27F99CECD94C7D4F3191D303BBEE37481C7AA15F233BA755E9E4376345A9A67E7994BDC1C680BB3522D8C93EB0CCC91AD31AD450DA30D337662D19AC03E2B4EF5F6EC18282D491E19767D7B24542DFDEFF6F62185503532069BBB369E3BB9FB19AC6F1C30B97D249EEE764E0BAC97F25C873D973953E5153A42064BBFABFD06A4BB486860BF6637406C9FC36813A4A75F75C31CCA9F69F8DE59ADECEF6BDE7E07800FCBE035D3176AF8473E23E9AA3DFEE221196D1148302677C720CFE2544A03DB553E7F1B8427BA1CC72B0F29B12DFEF4C081D076D353E71880AADFF386352AF0AB7B28ED49E1E672D11F9</Modulus>
            <Exponent>03</Exponent>
            <ExpiryDate>1250</ExpiryDate>
            <Index>245</Index>
            <ActivationDate />
            <Checksum>C2239804C8098170BE52D6D5D4159E81CE8466BF</Checksum>
            <RID>A000000004</RID>
          </CAPublicKeyCertificate>
        </ArrayOfCAPublicKeyCertificate>
        ";
        #endregion

        #region Revoked Certs
        public const string RevokedCerts = @"<?xml version = ""1.0"" encoding=""utf-8""?>
        <ArrayOfRevokedCAPublicKeyCertificate xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
        </ArrayOfRevokedCAPublicKeyCertificate>
        ";
        //<RevokedCAPublicKeyCertificate>
        //    <ExpiryDate>1220</ExpiryDate>
        //    <Index>173</Index>
        //    <RID>A000000004</RID>
        //</RevokedCAPublicKeyCertificate>
        #endregion

        #region Hot Cards
        public const string ExceptionFile = @"<?xml version = ""1.0"" encoding=""utf-8""?>
        <ArrayOfHotCard xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
        </ArrayOfHotCard>
        ";

        //<HotCard>
        // <PAN>1234567890123456</PAN>
        //</HotCard>
        #endregion

        #region Terminal Global Config
        public const string TerminalConfigurationData = @"<?xml version = ""1.0"" encoding=""utf-8""?>
        <ArrayOfTLVXML xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
          <TLVXML>
            <Tag>DF8112</Tag>
            <Value />
          </TLVXML>
          <TLVXML>
            <Tag>FF8102</Tag>
            <Children />
          </TLVXML>
          <TLVXML>
            <Tag>FF8103</Tag>
            <Children />
          </TLVXML>
          <TLVXML>
            <Tag>DF8110</Tag>
            <Value>01</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F7C</Tag>
            <Value></Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F53</Tag>
            <Value>32</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F45</Tag>
            <Value>0000</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F4C</Tag>
            <Value>0000</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F39</Tag>
            <Value>00</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F1A</Tag>
            <Value>0710</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F1D</Tag>
            <Value>00</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F1B</Tag>
            <Value>20000</Value>
          </TLVXML>
          <TLVXML>
            <Tag>5F2A</Tag>
            <Value>0710</Value>
          </TLVXML>
          <TLVXML>
            <Tag>5F36</Tag>
            <Value>02</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F7A</Tag>
            <Value>00</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F66</Tag>
            <Value>00000000</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F15</Tag>
            <Value>0000</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F4E</Tag>
            <Value>2020</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F5C</Tag>
            <Value>0000000000000000</Value>
          </TLVXML>
        </ArrayOfTLVXML>
        ";
        #endregion

        #region Terminal Contact Config
        public const string TerminalSupportedContactAIDs = @"<?xml version = ""1.0"" encoding=""utf-8""?>
        <ArrayOfTerminalSupportedContactKernelAidTransactionTypeCombination xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
            <TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A0000000046000</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>

            <TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A0000000031010</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>

           <!--<TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A000000003101008</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>
            <TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A000000003101009</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>-->
           
            <!--<TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A000000003101005</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>
            <TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A000000003101004</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>
            <TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A000000003101003</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>
            <TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A000000003101002</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>
            <TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A000000003101001</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>-->
           
            <TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A0000000032010</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>
            <TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A0000000032020</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>
            <TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A0000000033010</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>
            <TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A0000000034010</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>
            <TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A0000000035010</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>
            <TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A0000000041010</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>
            <!--<TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A000000004101001</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>
            <TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A000000004101002</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>-->
            <TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A0000000042010</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>
            <TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A0000000043010</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>
            <TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A0000000043060</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>
            <TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A0000000044010</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>
            <TerminalSupportedContactKernelAidTransactionTypeCombination>
                <AIDEnum>A0000000045010</AIDEnum>
                <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
            </TerminalSupportedContactKernelAidTransactionTypeCombination>
        </ArrayOfTerminalSupportedContactKernelAidTransactionTypeCombination>
        ";
        #endregion

        #region Terminal Contactless Config
        public const string TerminalSupportedContactlessRIDs = @"<?xml version = ""1.0"" encoding=""utf-8""?>
        <ArrayOfTerminalSupportedContactlessKernelAidTransactionTypeCombination xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
          <TerminalSupportedContactlessKernelAidTransactionTypeCombination>
            <KernelEnum>Kernel2</KernelEnum>
            <RIDEnum>A000000004</RIDEnum>
            <TransactionTypeEnum>PurchaseGoodsAndServices</TransactionTypeEnum>
            <StatusCheckSupportFlag xsi:nil=""true"" />
            <ZeroAmountAllowedFlag xsi:nil=""true"" />
            <ReaderContactlessTransactionLimit xsi:nil=""true"" />
            <ReaderContactlessFloorLimit xsi:nil=""true"" />
            <TerminalFloorLimit_9F1B xsi:nil=""true"" />
            <ReaderCVMRequiredLimit xsi:nil=""true"" />
            <ExtendedSelectionSupportFlag xsi:nil=""true"" />
            <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
          </TerminalSupportedContactlessKernelAidTransactionTypeCombination>
          <TerminalSupportedContactlessKernelAidTransactionTypeCombination>
            <KernelEnum>Kernel2</KernelEnum>
            <RIDEnum>A000000004</RIDEnum>
            <TransactionTypeEnum>PurchaseWithCashback</TransactionTypeEnum>
            <StatusCheckSupportFlag xsi:nil=""true"" />
            <ZeroAmountAllowedFlag xsi:nil=""true"" />
            <ReaderContactlessTransactionLimit xsi:nil=""true"" />
            <ReaderContactlessFloorLimit xsi:nil=""true"" />
            <TerminalFloorLimit_9F1B xsi:nil=""true"" />
            <ReaderCVMRequiredLimit xsi:nil=""true"" />
            <ExtendedSelectionSupportFlag xsi:nil=""true"" />
            <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
          </TerminalSupportedContactlessKernelAidTransactionTypeCombination>
          <TerminalSupportedContactlessKernelAidTransactionTypeCombination>
            <KernelEnum>Kernel2</KernelEnum>
            <RIDEnum>A000000004</RIDEnum>
            <TransactionTypeEnum>Refund</TransactionTypeEnum>
            <StatusCheckSupportFlag xsi:nil=""true"" />
            <ZeroAmountAllowedFlag xsi:nil=""true"" />
            <ReaderContactlessTransactionLimit xsi:nil=""true"" />
            <ReaderContactlessFloorLimit xsi:nil=""true"" />
            <TerminalFloorLimit_9F1B xsi:nil=""true"" />
            <ReaderCVMRequiredLimit xsi:nil=""true"" />
            <ExtendedSelectionSupportFlag xsi:nil=""true"" />
            <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
          </TerminalSupportedContactlessKernelAidTransactionTypeCombination>
          <TerminalSupportedContactlessKernelAidTransactionTypeCombination>
            <TTQAsBytes>9F660427C04000</TTQAsBytes>
            <KernelEnum>Kernel3</KernelEnum>
            <RIDEnum>A000000050</RIDEnum>
            <TransactionTypeEnum>PurchaseGoodsAndServices</TransactionTypeEnum>
            <StatusCheckSupportFlag>false</StatusCheckSupportFlag>
            <ZeroAmountAllowedFlag>true</ZeroAmountAllowedFlag>
            <ReaderContactlessTransactionLimit>20000</ReaderContactlessTransactionLimit>
            <TerminalFloorLimit_9F1B>10000</TerminalFloorLimit_9F1B>
            <ReaderCVMRequiredLimit>3000</ReaderCVMRequiredLimit>
            <ReaderContactlessFloorLimit>2000</ReaderContactlessFloorLimit>
            <ExtendedSelectionSupportFlag>false</ExtendedSelectionSupportFlag>
            <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
          </TerminalSupportedContactlessKernelAidTransactionTypeCombination>
          <TerminalSupportedContactlessKernelAidTransactionTypeCombination>
            <TTQAsBytes>9F660427C04000</TTQAsBytes>
            <KernelEnum>Kernel3</KernelEnum>
            <RIDEnum>A000000003</RIDEnum>
            <TransactionTypeEnum>PurchaseGoodsAndServices</TransactionTypeEnum>
            <StatusCheckSupportFlag>false</StatusCheckSupportFlag>
            <ZeroAmountAllowedFlag>true</ZeroAmountAllowedFlag>
            <ReaderContactlessTransactionLimit>20000</ReaderContactlessTransactionLimit>
            <TerminalFloorLimit_9F1B>10000</TerminalFloorLimit_9F1B>
            <ReaderCVMRequiredLimit>3000</ReaderCVMRequiredLimit>
            <ReaderContactlessFloorLimit>2000</ReaderContactlessFloorLimit>
            <ExtendedSelectionSupportFlag>false</ExtendedSelectionSupportFlag>
            <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
          </TerminalSupportedContactlessKernelAidTransactionTypeCombination>
          <TerminalSupportedContactlessKernelAidTransactionTypeCombination>
            <TTQAsBytes>9F660427C04000</TTQAsBytes>
            <KernelEnum>Kernel3</KernelEnum>
            <RIDEnum>A000000003</RIDEnum>
            <TransactionTypeEnum>PurchaseWithCashback</TransactionTypeEnum>
            <StatusCheckSupportFlag>false</StatusCheckSupportFlag>
            <ZeroAmountAllowedFlag>true</ZeroAmountAllowedFlag>
           <ReaderContactlessTransactionLimit>20000</ReaderContactlessTransactionLimit>
            <TerminalFloorLimit_9F1B>10000</TerminalFloorLimit_9F1B>
            <ReaderCVMRequiredLimit>3000</ReaderCVMRequiredLimit>
            <ReaderContactlessFloorLimit>2000</ReaderContactlessFloorLimit>
             <ExtendedSelectionSupportFlag>false</ExtendedSelectionSupportFlag>
            <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
          </TerminalSupportedContactlessKernelAidTransactionTypeCombination>
          <TerminalSupportedContactlessKernelAidTransactionTypeCombination>
            <TTQAsBytes>9F660427C04000</TTQAsBytes>
            <KernelEnum>Kernel3</KernelEnum>
            <RIDEnum>A000000003</RIDEnum>
            <TransactionTypeEnum>Refund</TransactionTypeEnum>
            <StatusCheckSupportFlag>false</StatusCheckSupportFlag>
            <ZeroAmountAllowedFlag>true</ZeroAmountAllowedFlag>
           <ReaderContactlessTransactionLimit>20000</ReaderContactlessTransactionLimit>
            <TerminalFloorLimit_9F1B>10000</TerminalFloorLimit_9F1B>
            <ReaderCVMRequiredLimit>3000</ReaderCVMRequiredLimit>
            <ReaderContactlessFloorLimit>2000</ReaderContactlessFloorLimit>
            <ExtendedSelectionSupportFlag>false</ExtendedSelectionSupportFlag>
            <ApplicationSelectionIndicator>true</ApplicationSelectionIndicator>
          </TerminalSupportedContactlessKernelAidTransactionTypeCombination>
        </ArrayOfTerminalSupportedContactlessKernelAidTransactionTypeCombination>
        ";
        #endregion

        #region Kernel Contact Config
        public const string KernelGlobalConfigurationData = @"<?xml version = ""1.0"" encoding=""utf-8""?>
        <KernelConfiguration xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">

        </KernelConfiguration>
        ";

        public const string KernelConfigurationData = @"<?xml version = ""1.0"" encoding=""utf-8""?>
        <ArrayOfTLVXML xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
          <TLVXML>
            <Tag>9F33</Tag>
            <Value>E0F8F8</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F40</Tag>
            <Value>0000000000</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F09</Tag>
            <Value>0002</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F1A</Tag>
            <Value>0710</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F35</Tag>
            <Value>22</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9C</Tag>
            <Value>00</Value>
          </TLVXML>
         <TLVXML>
            <Tag>DF8120</Tag>
            <Value>841080000C</Value>
            </TLVXML>
          <TLVXML>
          <Tag>DF8121</Tag>
            <Value>841080000C</Value>
            </TLVXML>
          <TLVXML>
            <Tag>DF8122</Tag>
            <Value>841080000C</Value>
          </TLVXML>
          <TLVXML>
            <Tag>DF812D</Tag>
            <Value>000013</Value>
          </TLVXML>
        </ArrayOfTLVXML>
        ";
        #endregion

        #region Kernel 1 Contacless Config
        public const string Kernel1GlobalConfigurationData = @"<?xml version = ""1.0"" encoding=""utf-8""?>
        <Kernel1Configuration xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
          <VLPTerminalSupportIndicator>true</VLPTerminalSupportIndicator>
        </Kernel1Configuration>
        ";
        public const string Kernel1ConfigurationData = @" <?xml version = ""1.0"" encoding=""utf-8""?>
        <ArrayOfTLVXML xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
          <TLVXML>
            <Tag>9F33</Tag>
            <Value>E0F8F8</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F40</Tag>
            <Value>0000000000</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F09</Tag>
            <Value>0002</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F1A</Tag>
            <Value>0710</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F35</Tag>
            <Value>22</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9C</Tag>
            <Value>00</Value>
          </TLVXML>

        </ArrayOfTLVXML>
        ";
        #endregion

        #region Kernel 2 Contacless Config
        public const string Kernel2ConfigurationData = @"<?xml version = ""1.0"" encoding=""utf-8""?>
        <ArrayOfTLVXML xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
            <TLVXML>
            <Tag>9F35</Tag>
            <Value>22</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF811E</Tag>
            <Value>F0</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF812C</Tag>
            <Value>F0</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF8118</Tag>
            <Value>68</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF8119</Tag>
            <Value>08</Value>
            </TLVXML>
            <!-- TERMINAL_ACTION_CODE_DEFAULT_DF8120_KRN2 -->
            <TLVXML>
            <Tag>DF8120</Tag>
            <Value>841080800C</Value>
            </TLVXML>
            <!-- TERMINAL_ACTION_CODE_DENIAL_DF8121_KRN2 send online if possible -->
            <TLVXML>
            <Tag>DF8121</Tag>
            <Value>801000000C</Value>
            </TLVXML>
            <!-- TERMINAL_ACTION_CODE_ONLINE_DF8122_KRN2 -->
            <TLVXML>
            <Tag>DF8122</Tag>
            <Value>841080800C</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF810D</Tag>
            <Value>02</Value>
            </TLVXML>
            <TLVXML>
            <Tag>9F5C</Tag>
            <Value>0000000000000001</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF60</Tag>
            <Value>0000000000000000</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF8109</Tag>
            <Value>0000000000000000</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF63</Tag>
            <Value>00000000</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF62</Tag>
            <Value>A8</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF8108</Tag>
            <Value>80</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF810A</Tag>
            <Value>E6</Value>
            </TLVXML>
            <!-- TERMINAL_CAPABILITIES_9F33_KRN E0F8F8-->
            <TLVXML>
            <Tag>9F33</Tag>
            <Value>E0F8F8</Value>
            </TLVXML>
            <TLVXML>
            <Tag>9F40</Tag>
            <Value>D200F0F3FF</Value>
            </TLVXML>
            <TLVXML>
            <Tag>9F09</Tag>
            <Value>0002</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF8117</Tag>
            <Value>00</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF811A</Tag>
            <Value>9F6A04</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF8130</Tag>
            <Value>0D</Value>
            </TLVXML>
            <!-- TERMINAL_RISK_MANAGEMENT_DATA_9F1D_KRN2 20-->
            <TLVXML>
            <Tag>DF811B</Tag>
            <Value>00</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF810C</Tag>
            <Value>02</Value>
            </TLVXML>
            <TLVXML>
            <Tag>9F6D</Tag>
            <Value>0001</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF811C</Tag>
            <Value>012C</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF811D</Tag>
            <Value>00</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF812D</Tag>
            <Value>000013</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF811F</Tag>
            <Value>00</Value>
            </TLVXML>
            <!-- READER_CONTACTLESS_FLOOR_LIMIT_DF8123_KRN2 -->
            <TLVXML>
            <Tag>DF8123</Tag> 
            <Value>000000002000</Value>
            </TLVXML>
            <!-- READER_CONTACTLESS_TRANSACTION_LIMIT_NO_ONDEVICE_CVM_DF8124_KRN2 -->
            <TLVXML>
            <Tag>DF8124</Tag>
            <Value>000000004000</Value>
            </TLVXML>
            <!-- READER_CONTACTLESS_TRANSACTION_LIMIT_ONDEVICE_CVM_DF8125_KRN2 -->
            <TLVXML>
            <Tag>DF8125</Tag>
            <Value>000000003500</Value>
            </TLVXML>
            <!-- READER_CVM_REQUIRED_LIMIT_DF8126_KRN2 -->
            <TLVXML>
            <Tag>DF8126</Tag>
            <Value>000000003000</Value>
            </TLVXML>
            <TLVXML>
            <Tag>9F1A</Tag>
            <Value>0710</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF8127</Tag>
            <Value>01F4</Value>
            </TLVXML>
            <TLVXML>
            <Tag>9C</Tag>
            <Value>00</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF8133</Tag>
            <Value>0032</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF8132</Tag>
            <Value>0014</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF8134</Tag>
            <Value>0012</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF8135</Tag>
            <Value>0018</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF8136</Tag>
            <Value>012C</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF8137</Tag>
            <Value>012C</Value>
            </TLVXML>
            <TLVXML>
            <Tag>9F1D</Tag>
            <Value>0000000000000000</Value>
            </TLVXML>
            <TLVXML>
            <Tag>DF8131</Tag>
            <Value>00080000080020000004000004002000000100000100200000020000020020000000000000000700</Value>
            </TLVXML>
        </ArrayOfTLVXML>
        ";
        #endregion

        #region Kernel 3 Contacless Config
        public const string Kernel3GlobalConfigurationData = @"<?xml version = ""1.0"" encoding=""utf-8""?>
        <Kernel3Configuration xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
          <IDSSupported>false</IDSSupported>
          <FDDAForOnlineSupported>true</FDDAForOnlineSupported>
          <SDAForOnlineSupported>true</SDAForOnlineSupported>
          <DisplayAvailableSpendingAmount>true</DisplayAvailableSpendingAmount>
          <AUCManualCheckSupported>true</AUCManualCheckSupported>
          <AUCCashbackCheckSupported>true</AUCCashbackCheckSupported>
          <ATMOfflineCheck>true</ATMOfflineCheck>
          <ExceptionFileEnabled>true</ExceptionFileEnabled>
        </Kernel3Configuration>
        ";
        public const string Kernel3ConfigurationData = @"<?xml version = ""1.0"" encoding=""utf-8""?>
        <ArrayOfTLVXML xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
          <TLVXML>
            <Tag>9F33</Tag>
            <Value>E0F8F8</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F40</Tag>
            <Value>0000000000</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F09</Tag>
            <Value>0002</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F1A</Tag>
            <Value>0710</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9F35</Tag>
            <Value>22</Value>
          </TLVXML>
          <TLVXML>
            <Tag>9C</Tag>
            <Value>00</Value>
          </TLVXML>
        </ArrayOfTLVXML>
        ";
        #endregion
    }
}
