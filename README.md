<h1 align="center">
 Sunshine Attractions
</h1>

<p align="center">
  <a href="#-about-the-project">About the project</a>&nbsp;&nbsp;&nbsp;|&nbsp;&nbsp;&nbsp;
  <a href="#-technologies">Technologies</a>&nbsp;&nbsp;&nbsp;|&nbsp;&nbsp;&nbsp;
  <a href="#-getting-started">Getting started</a>&nbsp;&nbsp;&nbsp;|&nbsp;&nbsp;&nbsp;
  <a href="#-how-to-contribute">How to contribute</a>&nbsp;&nbsp;&nbsp;|&nbsp;&nbsp;&nbsp;
  <a href="#-license">License</a>
</p>

## 👨🏻‍💻 Welcome to Sunshine Attractions!

<p align="left" style="color: red;"> Sunshine Attractions is a full-stack review application designed for theme parks in Florida, the Sunshine State. It provides tourists and locals with up-to-date information on rides, shows, and food options across various Florida theme parks. Users can track live wait times for rides and leave reviews about different attractions within the park.

A map feature is also in development, which will allow users to share their location within the park. Based on their proximity to rides and current wait times, users will receive personalized recommendations on which attractions to visit next.</p>

## 🔆 Home Page

<p align="left">
 Welcome to Sunshine Attractions, below you will find the home page that consists of various theme parks in Florida. Users have plenty of options to start exploring new attractions, food, and entertainment.
</p>

## 🔆 Selection Page

<p align="left">
 Discover rides, food, and shows at various theme parks. Dynamically renders information from the backend for each page.
</p>

<p align="left">
<a href="https://ibb.co/092QsmM"><img src="https://i.ibb.co/PWjtDFh/Selection.jpg" alt="Selection" border="0"></a>
</p>

## 🔆 Review Page

<p align="left">
This is what a user review looks like. Reviews show the user profile picture, along with their username, and star rating. You can see their thoughts about the specific attractions, food, or show in the description box. The review componets support likes, comments from other users, and comment likes / dislikes.
</p>

<p align="left">
<a href="https://ibb.co/zG2CDCP"><img src="https://i.ibb.co/xJgwdwz/Review.jpg" alt="Review" border="0"></a><br />
</p>

## 🔆 Filter reviews
<p align="left">
Filter reviews based on options such as: All reviews, Your reviews, Highest rated, Most critcal, and Recent reviews.
</p>

<p align="left">
<a href="https://ibb.co/hL2tJ6Y"><img src="https://i.ibb.co/34SL93R/a4a238090f23ce0914d981f1814fcf68.png" alt="a4a238090f23ce0914d981f1814fcf68" border="0"></a>
</p>

## 🔆 Profile Features
<p align="left">
Delete your reviews and comments, and change your profile picture in the profile menu. (More features to be added).
</p>

<p align="left">
<a href="https://ibb.co/S3cgZBr"><img src="https://i.ibb.co/87bwQ9c/Your-Reviews.png" alt="Your-Reviews" border="0"></a>
</p>

## 🔆 Park Maps
<p align="left">
View ride locations on the park map. (This feature will continue to be fleshed out, upcoming features include users being allowed to share their location on their park visit, then the users will be shown the closest rides to them with the lowest wait times).
</p>

<p align="left">
<a href="https://ibb.co/JzZZzTn"><img src="https://i.ibb.co/THyyHGc/maps.png" alt="maps" border="0"></a><br>
</p>

## 🚀 Technologies

Technologies that I used to develop this app

- [ASP.NET Core 8](https://dotnet.microsoft.com/apps/aspnet)
- [C#](https://learn.microsoft.com/dotnet/csharp/)
- [MongoDB](https://www.w3schools.com/mongodb/)
- [Razor Views](https://learn.microsoft.com/aspnet/core/mvc/views/razor)
- [JavaScript](https://www.javascript.com/) (browser behavior only)
- [Tailwind](tailwindcss.com)
- [Cloudinary]([https://cloudinary.com/])

## 💻 Getting started - Setting up your environment.

1. Clone the files and install the .NET 8 SDK.
2. Set `ConnectionStrings__MongoDb` to your MongoDB URI.
3. Configure Cloudinary and Google Maps via the environment-variable names in the ASP.NET Core migration section below.
4. Run `dotnet restore`, then `dotnet run`.

## 🤔 How to contribute

If you would like to contribute to this project please reach out to me via discord: chriscodes

**Follow the steps below**

```bash
0. Leave a comment on the issue you would like to work on 

1. Fork the original repository (top right corner next to watch and star buttons)

2. Under the dropdown menu from the button "code" copy the HTTPS link (from your forked repository) 'https://github.com/(your username)/Sunshine-attractions.git'

3. In the place you want to clone the project, use git clone (your https link here)

4. Once you have the project open in VSCODE use 'git remote add upstream  https://github.com/ChrisMunozCodes/Sunshine-attractions.git' in the terminal, this will track the main repository 

5. You can now use 'git fetch upstream' in the terminal to see a list of the different branches.

6. Use 'git checkout -b branch-name' replace branch-name with your branch. This will create a new branch for you to work within

7.. You can now use git add . & git commit -m '' 

8. Use 'git push -u origin a-descriptive-branch-name' replace a-descriptive-branch-name with your branch name (this will push all your code)

9. Now go back to your github and a button will appear that prompts you to make a pull request
```

# ASP.NET Core migration

This repository now runs as an ASP.NET Core 8 MVC application rather than the Express/EJS application. The new application entry point is `Program.cs`, with Razor views in `Views/` and C# controllers in `Controllers/`.

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
2. Configure the values in `appsettings.json` for local development, or use environment variables:
   - `ConnectionStrings__MongoDb`
   - `Cloudinary__CloudName`, `Cloudinary__ApiKey`, `Cloudinary__ApiSecret`
   - `GoogleMaps__ApiKey`, `GoogleMaps__MapId`
3. Run `dotnet restore` followed by `dotnet run`.

The MongoDB document field names and bcrypt password hashes are compatible with the original Node application, so the existing database can be reused. The old Node/EJS source is retained only as a migration reference; it is no longer the application entry point.

## Deploying to Render

The repository includes a `Dockerfile`, because Render deploys .NET applications through its Docker runtime. Create a **Web Service** from your GitHub repository, select **Docker**, then add these environment variables in Render's **Environment** tab (do not upload `config/.env`):

- `ConnectionStrings__MongoDb`
- `GoogleMaps__ApiKey`
- `GoogleMaps__MapId`

Render supplies `PORT` automatically. Use `/` as the health-check path.

# Things to add (Wish list)
- ~~Maps of theme parks with wait time markers and user geolocation (High Priority).~~
- Lowest wait time rides near the user based on the users location within the theme park map. (High Priority).
- ~~Mobile Layout (Responsiveness for mobile layouts) (High Priority).~~
- Following and followers feature (Med Priority).
- Trip Planner (Low Priority). - Fill out a personal calendar with different rides, food, and shows.
- Dark Mode? (Low Priority).
- ~~Comments.~~
- ~~Filter reviews~~
- ~~Likes.~~
- ~~Adding images to reviews.~~


---
