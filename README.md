#Contoso Security Demo
##Contents
1. [Getting Started] (#getting-started) 
	* Generate Application ID and Secret
	* Retrieve TenantID
	* Deploy to Azure
	* Download Security Demo Queries
2. [Azure SQL Security Features] (#azure-sql-security-features) 
	* Auditing & Threat Detection
	* Always Encrypted 
	* Row Level Security 
	* Data Masking

##Getting Started
###Generate Application ID and Secret
In order to allow your client application to access and use the keys in your Azure Key Vault, we need to provision a Client ID and Secret that your app will use to authenticate.&nbsp; To do this, head to the [Azure Portal] (https://manage.windowsazure.com/) and log in.

Select &ldquo;Active Directory&rdquo; in the left sidebar, choose the Active Directory you wish to use (or create a new one if it doesn&rsquo;t exist), then click &ldquo;Applications&rdquo;.

Add a new application by filling out the modal window that appears.

Enter a name, select &ldquo;Web Application&rdquo; as the type, and enter any URL for the Sign-On URL and App ID URI.&nbsp; These must include &ldquo;http://&rdquo;, but do not need to be real pages.&nbsp; 

Go to the &ldquo;Configure&rdquo; tab and generate a new client key (also called a &ldquo;secret&rdquo;) by selecting a duration from the dropdown, then saving the configuration.&nbsp; <strong>Copy the client ID and secret out to a text file</strong>, as they&rsquo;ll be used in deployment and in enabling the Always Encrypted functionality.
###Retrieve TenantID

In order to deploy an Azure Key Vault for use with the Always Encrypted functionality of the demo, you will need to provide your tenantID during the deployment process. This can be copied from Powershell in the response to the `Login-AzureRmAccount` command. After the deployment step, this information is not saved by the application. 

###Deploy to Azure 
Click the Deploy to Azure Button and fill out the fields to deploy the demo to your Azure Subscription

[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)

## Azure SQL Security Features 
### Auditing & Threat Detection
//TODO
### Always Encrypted 
####Grant Yourself Create Key Priveleges to your Azure Key Vault
+ Open Powershell
+ Login to your Azure account

	`Login-AzureRmAccount`
+ List your subscriptions

	`Get-AzureRmSubscription`
+ Select the subscription you used in deployment

	`Select-AzureRmSubscription -SubscriptionName <your subscription name>`
+ List your Azure Key Vaults associated with the Subscription

	`Get-AzureRmKeyVault`
+ Set the Access policy of your key vault (Choose the one that begins with "contosoakv")

	`Set-AzureRmKeyVaultAccessPolicy -VaultName "<your vault name> -ResourceGroupName "<your resource group>" -PermissionsToKeys create,get,wrapKey,unwrapKey,sign,verify -UserPrincipalName "<your Azure username>"`

####Enable Always Encrypted
+ Connect to your deployed database using SSMS: [Instructions]()
+ Encrypt Sensitive Data Columns using the Column Encryption Wizard 
	- Right click on the **Patients** table in the **Clinic** database and select **Encrypt Columns...**
	- The Column Encryption wizard will open. Click **Next**.
	- Select the **SSN** and **BirthDate** columns. 
		* Select **Deterministic Encryption** for **SSN** as the application needs to be able to search patients by SSN; Deterministic Encryption preserves that functionality for our app without leaving data exposed. 
		* Select **Randomized Encryption** for *BirthDate** 
	- Leave **CEK_Auto1 (New)** as the Key for both columns. Click **Next**.
	- On the **Master Key Configuration** page, set the Master Key Source to **Azure Key Vault**, select the Subscription you used in the deployment of the application, and select the key vault you selected  Click **Next**. 
	- Click the **Next** button on the Validation page.
	- The Summary Page provides an overview of the settings we selected. Click **Finish**. 
	- Monitor the progress of the wizard; once finished, click **Close**. 
+ View the data in SSMS (feel free to use **Select Top 1000 Rows**) 
	- Note that the data is now encrypted in both the **SSN** and **BirthDate** columns. 
+ Navigate to/Refresh the /patients page
	- The page should cause an error because we need to give the app permission to use the Key Vault. 
	- Use this command in Powershell (Remember registering the app with Active directory? Use the Client ID as the ServicePrincipalName. ) 
	
	`Set-AzureRmKeyVaultAccessPolicy -VaultName "<your vault name>" -ResourceGroupName "<your resource group>" -ServicePrincipalName "<your client ID>" -PermissionsToKeys get,wrapKey,unwrapKey,sign,verify` 
	- Refresh the page. Notice you didn't need to make any changes to the application. 
####How did that work? 
//TODO

### Row Level Security (RLS) 

####Login to the application 
Sign in using (Rachel@contoso.com/Password1!) or (alice@contoso.com/Password1!)

####Enable Row Level Security (RLS) 
+ Connect to your deployed database using SSMS: [Instructions]()
+ Open Enable-RLS.sql ( [Download it here]())
+ Execute the commands 
+ Observe the changes to the results returned on the /visits or /patients page

#### How did that work? 

#####The application leverages an Entity Framework feature called **interceptors** 
Specifically, we used a `DbConnectionInterceptor`. The `Opened()` function is called whenever Entity Framework opens a connection and we set SESSION_CONTEXT with the current application `UserId` there. 

##### Predicate functions
The predicate functions we created in Enable-RLS.sql identify users by the `UserId` which was set by our interceptor whenever a connection is established from the application. The two types of predicates we created were **Filter** and **Block**. 
+ **Filter** predicates silently filter `SELECT`, `UPDATE`, and `DELETE` operations to exclude rows that do not satisfy the predicate. 
+ **Block** predicates explicitly block (throw errors) on `INSERT`, `UPDATE`, and `DELETE` operations that do not satisfy the predicate. 

### Data Masking

#### Enable Data Masking
+ Navigate to the /patients page
+ Connect to your deployed database using SSMS: [Instructions]()
+ Open Enable-DDM.sql ([Download it here]()) 
+ Execute the commands
+ Observe the changes in results returned on the /visits page

#### How did that work? 
//TODO 
