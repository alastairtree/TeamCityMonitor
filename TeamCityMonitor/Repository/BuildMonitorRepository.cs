using System;
using System.Collections.Generic;
using System.Linq;
using BuildMonitor.Models;
using BuildMonitor.Properties;
using TeamCitySharp;
using TeamCitySharp.DomainEntities;

namespace BuildMonitor.Repository
{
    public class BuildMonitorRepository : IDisposable
    {
        private List<BuildConfig> buildConfigs;
        private TeamCityClient client;
        private List<BuildConfig> currentProjectConfigs;
        private List<ProjectModel> projectList;
        private List<Project> projects;

        public BuildMonitorRepository(string hostName)
        {
            client = new TeamCityClient(hostName, false);

            projectList = new List<ProjectModel>();
            projects = GetAllProjects();
            buildConfigs = GetAllBuildConfigs();
        }

        public void Dispose()
        {
            client = null;
            projectList = null;
            projects = null;
            buildConfigs = null;
        }

        public List<ProjectModel> GetPageOfProjectSummary(out bool morePages, int page = 1, int items = 25)
        {
            try
            {
                morePages = projects.Count > (items * page);

                foreach (var proj in projects.Skip((page-1) * items).Take(items))
                {
                    var skipRemainingBuildConfigs = false;

                    try
                    {
                        currentProjectConfigs = (from configs in buildConfigs
                            where configs.ProjectId == proj.Id
                            select configs).ToList();


                        foreach (var currentConfig in currentProjectConfigs)
                        {
                            if (!skipRemainingBuildConfigs)
                            {
                                var build = GetLatestBuildForConfig(currentConfig.Id);

                                var project = new ProjectModel
                                {
                                    ProjectName = proj.Name,
                                    ProjectId = proj.Id,
                                    BuildConfigName = currentConfig.Name,
                                    LastBuildTime = build.StartDate.ToString("dd/MM/yyyy HH:mm:ss"),
                                    LastBuildStatus = build.Status,
                                    LastBuildStatusText = build.StatusText,
                                    Url = build.WebUrl
                                };
                                if (Settings.Default.ShowFailedBuildsOnly)
                                {
                                    //if the last build was cancelled but we suceeded before that?
                                    if (build.Status == "UNKNOWN")
                                    {
                                        if (HadSucessfulBuildSinceLastFailure(currentConfig.Id))
                                        {
                                            continue;
                                        }
                                    }

                                    if (build.Status != "SUCCESS" && build.StartDate > DateTime.Today.AddYears(-1) &&
                                        build.Number != "N/A")
                                    {
                                        projectList.Add(project);

                                        if (Settings.Default.ShowOneFailPerProject)
                                        {
                                            skipRemainingBuildConfigs = true;
                                        }
                                    }
                                }
                                else
                                {
                                    projectList.Add(project);
                                }
                            }
                        }
                    }
                    catch (ArgumentNullException)
                    {
                        var project = new ProjectModel
                        {
                            ProjectName = proj.Name,
                            ProjectId = proj.Id,
                            BuildConfigName = "** No Builds available **",
                            LastBuildTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                            LastBuildStatus = "undefined",
                            LastBuildStatusText = String.Empty
                        };
                        projectList.Add(project);
                    }
                    catch (NullReferenceException)
                    {
                        var project = new ProjectModel
                        {
                            ProjectName = proj.Name,
                            ProjectId = proj.Id,
                            BuildConfigName = String.Empty,
                            LastBuildTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                            LastBuildStatus = "undefined",
                            LastBuildStatusText = String.Empty
                        };
                        projectList.Add(project);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                return projectList
                    .OrderBy(x => GetStatus(x).Contains("success") ? 3 : 0)
                    .ThenBy(x => GetStatus(x).Contains("undefined") ? 2 : 0)
                    .ThenBy(x => GetStatus(x).Contains("unknown") ? 2 : 0)
                    .ThenBy(x => GetStatus(x).Contains("failure") ? 1 : 0)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetStatus(ProjectModel model)
        {
            return string.IsNullOrEmpty(model.LastBuildStatus) ? "" : model.LastBuildStatus.ToLower();
        }

        private List<Project> GetAllProjects()
        {
            client.Connect(
                Settings.Default.TeamCityUser,
                Settings.Default.TeamCityPwd,
                Settings.Default.TeamCityIsGuest
                );


            return client.AllProjects();
        }

        private List<BuildConfig> GetAllBuildConfigs()
        {
            client.Connect(
                Settings.Default.TeamCityUser,
                Settings.Default.TeamCityPwd,
                Settings.Default.TeamCityIsGuest
                );
            return client.AllBuildConfigs();
        }

        private Build GetLatestBuildForConfig(string configId)
        {
            client.Connect(
                Settings.Default.TeamCityUser,
                Settings.Default.TeamCityPwd,
                Settings.Default.TeamCityIsGuest
                );
            return client.LastBuildByBuildConfigId(configId);
        }

        private bool HadSucessfulBuildSinceLastFailure(string configId)
        {
            client.Connect(
                Settings.Default.TeamCityUser,
                Settings.Default.TeamCityPwd,
                Settings.Default.TeamCityIsGuest
                );
            var sucess = client.LastSuccessfulBuildByBuildConfigId(configId);
            var failed = client.LastFailedBuildByBuildConfigId(configId);


            return sucess.StartDate > failed.StartDate;
        }
    }
}