# https://docs.microsoft.com/en-us/powershell/module/pkiclient/new-selfsignedcertificate?view=win10-ps
# https://www.petri.com/create-self-signed-certificate-using-powershell


#New-SelfSignedCertificate

#New-SelfSignedCertificate -DnsName "www.fabrikam.com", "www.contoso.com" -CertStoreLocation "G:\Dev\Fuxion\Repo\src\sketches\DemoWpf\Resources"

#New-SelfSignedCertificate -Subject "CN=MyServer" -KeySpec Exchange -KeyUsage "DataEncipherment, KeyEncipherment, DigitalSignature" -Path G:\Dev\Fuxion\Repo\src\sketches\DemoWpf\Resources\example.pfx -Exportable -SAN "MyServer" -SignatureAlgorithm sha256 -AllowSMIME -Password (ConvertTo-SecureString "abc123dontuseme" -AsPlainText -Force) -NotAfter (get-date).AddYears(20)

#Variables
$dns = 'Fuxion.local'
$exportedFilePassword = 'fuxion'
$exportedFilePathPath = 'G:\Dev\Fuxion\Repo\src\sketches\DemoWpf\Resources\Fuxion.pfx'
$expirationDate = (Get-Date).AddYears(50)



$certStore = 'cert:\CurrentUser\My'
$cert = New-SelfSignedCertificate -CertStoreLocation $certStore -DnsName $dns -KeySpec KeyExchange -NotAfter $expirationDate
$certPath = $certStore + '\' + $cert.thumbprint 
$pwd = ConvertTo-SecureString -String $exportedFilePassword -Force -AsPlainText
Export-PfxCertificate -cert $certPath -FilePath $exportedFilePathPath -Password $pwd
Get-ChildItem $certPath  | Remove-Item