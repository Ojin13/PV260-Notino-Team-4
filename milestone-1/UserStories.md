# User Stories

## General

### US-01 User login
User must enter valid email to be able to interact with the app.  
Development cost: 3 man-hours

**Acceptance criteria**
- **AC1:**  
  Given the email prompt is shown  
  When the user enters valid email address  
  Then the user is granted access to the app

- **AC2:**  
  Given the email prompt is shown  
  When the user enters invalid email address  
  Then the system displays an error message and displays prompt again

---

### US-02 Option selection
User can choose options in tree-like hierarchy of options to access different system functionalities. The available options are shown in a format of selection prompt.  
Development cost: 5 man-hours

**Acceptance criteria**
- **AC1:**  
  Given the user is logged in  
  When the user selects a valid hierarchy endpoint  
  Then the user is moved to the endpoint

- **AC2:**  
  Given the user is logged in and has permissions to do so  
  When the user selects a valid function in a hierarchy  
  Then the system executes the called function

- **AC3:**  
  Given the user is logged in  
  When the user selects invalid hierarchy endpoint  
  Then app displays an error message and displays selection prompt again

---

## Admin

### US-03 Admin login
User must enter correct password to be able to access admin functionalities.  
Development cost: 3 man-hours

**Acceptance criteria**
- **AC1:**  
  Given the user is logged in using their email  
  When the user chooses to access admin functionalities  
  Then the app requests the password

- **AC2:**  
  Given the password prompt is displayed  
  When the user enters password 'JAHODOVÝDORT375'  
  Then the app permits access to admin functionalities

- **AC3:**  
  Given the password prompt is displayed  
  When the user enters incorrect password  
  Then the app displays an error message and denies access

---

### US-04 Report download and diff update
User with admin access can request app to download latest report and recalculate diff.  
Development cost: 13 man-hours

**Acceptance criteria**
- **AC1:**  
  Given the app has internet connection  
  When user with admin access requests report update  
  Then app downloads latest report and recalculates new diff

- **AC2:**  
  Given the app has no internet connection or source page is out of order  
  When user with admin access requests report update  
  Then app displays an error message and no changes are applied

---

### US-05 Export Diff
As a user with admin access, I want to export the calculated diff to a file so that I can review or share the changes externally.  
Development cost: 7 man-hours

**Acceptance criteria**
- **AC1:**  
  Given the user has admin access and a diff has been calculated  
  When the user requests to export the diff  
  Then the app prompts for a file path and exports the diff to the specified file

- **AC2:**  
  Given the user has admin access and a diff has been calculated  
  When the user provides an invalid or inaccessible file path  
  Then the app displays an error message and does not export the diff

- **AC3:**  
  Given the user does not have admin access  
  When the user attempts to export the diff  
  Then the app denies the action and displays an appropriate error message

---

## User

### US-06 Display history of diff. reports
Normal user can display list with history of stock differences, so he can stock market evolution and make some financial decisions  
Development cost: 4 man-hours

**Acceptance criteria**
- **AC1:**  
  Given: User is in home page and has tree selection menu opened  
  When: User selects in “User mode” “Diff. Report history” option  
  Then: Last diff. report is displayed

- **AC2:**  
  Given: User has open list of diff. report  
  When: Clicks on previous or following  
  Then: Previous or following diff. report is displayed, respectively

---

### US-07 Display diff. history for one specific company
Normal user can display history of stock differences for one specific company, so he can stock market evolution and make some financial decisions  
Development cost: 2 man-hours

**Acceptance criteria**
- **AC1:**  
  Given: User has diff. report history opened  
  When: User clicks option show history for given company  
  Then: Menu with available companies has displayed

- **AC2:**  
  Given: Menu with available companies is displayed  
  When: User selects company of interest  
  Then: Paginated diff. report history for given company is displayed

---

## Preklad

### US-08 Change of app language
All users can change language of application by their preference so they can better understand the navigation and menu options  
Development cost: 10 man-hours

**Acceptance criteria**
- **AC1:**  
  Given: User is in home page and has tree selection menu opened  
  When: User selects in “Change language” option  
  Then: Then menu with languages has opened

- **AC2:**  
  Given: User has selected preferred language  
  When: Clicks “OK”  
  Then: Language of the application has changed

---

## Nasadenie

### US-09 Download application
User can download the application (.exe file) from a simple UI page so they can install and run the app locally.  
Development cost: 6 man-hours

**Acceptance criteria**
- **AC1:**  
  Given the user opens the download page  
  When the page is loaded  
  Then the user sees list of available application versions

- **AC2:**  
  Given the user is on the download page  
  When the user selects a version and clicks download  
  Then the .exe file is downloaded to their device

- **AC3:**  
  Given the user is on the download page  
  When no versions are available  
  Then the system displays message “No versions available”

---

### US-10 Version list display
User can see multiple versions of the application so they can choose which version to download.  
Development cost: 4 man-hours

**Acceptance criteria**
- **AC1:**  
  Given the user opens the download page  
  When versions exist  
  Then the system displays list of versions (e.g. version number, date)

- **AC2:**  
  Given the version list is displayed  
  When a new version is added  
  Then it appears in the list automatically

---

### US-11 Deploy application on GitHub Pages
Developer can deploy the download page to GitHub Pages so users can access and download the application online.  
Development cost: 4 man-hours

**Acceptance criteria**
- **AC1:**  
  Given the application UI page is ready  
  When developer deploys the page to GitHub Pages  
  Then the page is publicly accessible via a URL

- **AC2:**  
  Given the page is deployed  
  When user opens the GitHub Pages link  
  Then the download page is displayed correctly

- **AC3:**  
  Given a new version of the app is added  
  When changes are pushed to repository  
  Then the GitHub Pages site is updated with the new version

---

## Audit

### US-12 Action logging
System logs all user actions so activity can be tracked later.  
Development cost: 6 man-hours

**Acceptance criteria**
- **AC1:**  
  Given any user performs an action  
  When the action is completed  
  Then the system stores log (user email, action, timestamp) in database

- **AC2:**  
  Given the database is available  
  When logs are created  
  Then they are persisted (saved permanently)

---

### US-13 View audit logs
Admin can view audit logs so they can monitor system usage.  
Development cost: 5 man-hours

**Acceptance criteria**
- **AC1:**  
  Given admin is logged in  
  When admin selects “View logs” option  
  Then list of logs is displayed

- **AC2:**  
  Given logs are displayed  
  When there are no logs  
  Then system shows message “No logs available”

---

### US-14 Filter audit logs
Admin can filter logs so they can find specific actions faster.  
Development cost: 5 man-hours

**Acceptance criteria**
- **AC1:**  
  Given admin is viewing logs  
  When admin filters by user email  
  Then only logs for that user are shown

- **AC2:**  
  Given admin is viewing logs  
  When admin filters by date or action  
  Then only matching logs are displayed

---

## Power-user

### US-15 Color Theme Management
As a Power User, I can change the color theme of the application, so that my local terminal application instance is more comfortable to work with.  
Development cost: 5 man-hours

**Acceptance criteria**
- **AC1:**  
  Given I am logged in as a Power User  
  When I navigate to the "Color Settings" section  
  Then I see a list of available color palettes, each showing a preview of its colors and the palette name.

- **AC2:**  
  Given I am viewing the list of color palettes  
  When I select a palette  
  Then the UI immediately redraws in the new colors without requiring an application restart.

- **AC3:**  
  Given I am viewing the list of color palettes  
  When I look at the currently selected palette  
  Then it is clearly distinguished from other palettes by a text indicator.

- **AC4:**  
  Given I have selected a palette and closed the application  
  When I relaunch the application  
  Then the application opens with the last selected palette applied.

- **AC5:**  
  Given I am logged in as a User or Admin  
  When I look at the main menu  
  Then the color change option is neither visible nor accessible.

- **AC6:**  
  Given I am logged in as a Power User and I select a new palette  
  When the palette change is confirmed  
  Then the action is written to the audit log including the user's email, the selected palette, and the timestamp of the change.

---

### US-16 Email report Distribution
As a Power User, I can enter a list of recipient email addresses, so that a diff report can be sent to them via email.  
Development cost: 6 man-hours

**Acceptance criteria**
- **AC1:**  
  Given I am logged in as a Power User  
  When I navigate to the "Send Report" section  
  Then I see a text field for entering recipient email addresses.

- **AC2:**  
  Given I am entering email addresses for the report  
  When I type multiple addresses  
  Then I can separate them using either commas or semicolons.

- **AC3:**  
  Given I have entered a list of email addresses and at least one is in an invalid format  
  When I confirm the recipient list  
  Then the application displays an error message specifying the invalid address and does not allow me to proceed.

- **AC4:**  
  Given I have left the address list empty  
  When I confirm the recipient list  
  Then the application displays an error message and does not allow sending.

- **AC5:**  
  Given I have entered valid email addresses  
  When I confirm the recipient list  
  Then the application displays a summary of the entered addresses and asks for confirmation before sending.

- **AC6:**  
  Given I am logged in as a User or Admin  
  When I look at the main menu  
  Then the option to send a report via email is neither visible nor accessible.

- **AC7:**  
  Given I have entered more than 100 email addresses  
  When I confirm the recipient list  
  Then the application informs me that the maximum is 100 recipients and the report will not be sent.

- **AC8:**  
  Given the recipient list has been confirmed and sending is initiated  
  When the SendGrid delivery fails due to any external error  
  Then the application displays an error message notifying me of the failure.

- **AC9:**  
  Given the report has been successfully sent  
  When the sending completes  
  Then the action is written to the audit log including the Power User's email, the timestamp, and the list of recipients.

- **AC10:**  
  Given no diff report currently exists  
  When I attempt to send a report  
  Then the application clearly informs me that I must create a diff first, explaining why sending is not possible.
