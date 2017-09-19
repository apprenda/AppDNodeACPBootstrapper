# Enabling AppDynamics integration for Node.js applications on Apprenda

As explained in https://github.com/apprenda/AppDynamicsIntegration, Apprenda supports the enhancing of applications running on Apprenda’s private platform as a service (PaaS) with AppDynamics monitoring. The integration is made possible by the advanced extensibility that Apprenda offers through bootstrap policies. This repository contains both the code and the binaries necessary to upload a new bootstrap policy in Apprenda and light up APM monitoring for Node.js applications.

<h3>Release Notes</h3>

+ Version 1.0

<h3>Features</h3>
With AppDynamics monitoring Apprenda applications, developers can:

- Monitor multi-instance distributed applications in real time
- Monitor how the application is interacting with the database and gain access to query latencies
- Monitor performance metrics and the overall health of an application
- Monitor Key Performance Indicators (KPIs) and define custom metrics that provide insight into the internal behavior of the application
- Monitor how end users are using an app and the experience they receive throughout the day
- Quickly diagnose bottlenecks in an application 
- Receive proactive alerts when the application is falling behind its SLA

<h3>License</h3>
Copyright (c) 2017 Apprenda Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

<h2>Additional Information</h2>
Application Bootstrap Policies (BSP) on the Apprenda Platform allow Platform Operators to set specific rules governing inclusion of specified libraries with the deployment of .Net UI, WCF/Windows Services, and Java Web Application components. These policies apply comparison logic set by the Platform Operator to match specified libraries to the desired application components. The ability to add and extend the functionality of guest applications through policy is one of the greatest and most powerful features of Apprenda.

Enhancing Apprenda applications to work with your standardized corporate-wide deployment of AppDynamics is simple through the power of Apprenda extensibility and bootstrap policies. To learn more about bootstrap policies, visit the following links:
- http://docs.apprenda.com/6-5/bootstrap-policies
- http://docs.apprenda.com/6-5/custom-bootstrapper
- http://apprenda.com/blog/customize-paas-application-bootstrap-policies/

To get a better understanding of this integration through a step-by-step video turorial, visit one of the two following links:
- https://apprenda.wistia.com/medias/ck8afudkk1
- https://www.youtube.com/watch?v=kKTsi5qsWSs

<h2>Contents</h2>

- AppDNodeBootstrapper contains the source code for the bootstrap policy in Apprenda
- Deployment contains a zip file that can be immediately uploaded to Apprenda as a bootstrap policy

<h2>Installation Steps</h2>

- Read the AppDynamicsInstallationGuideForApprenda.docx from https://github.com/apprenda/AppDynamicsIntegration
- Set up Apprenda, integrating it with Kubernetes as per http://docs.apprenda.com/7-0/kubernetes
- Set up AppDynamics in the environment, installing an agent on every node to be monitored
- Create the following Custom Properties in Apprenda, targeting applications, allowing custom values to be entered by developers: AppdController, AppdAccount, AppdKey, AppdAppName, and AppdAppTier
- Create the following Custom Property in Apprenda, APMEnable, targeting applications. There should only be two possible values: Yes and No. Developers can pick Yes to bootstrap APM monitoring and No if they don't APM monitoring
- Create a new bootstrap policy in Apprenda, targeting Kubernetes Components, using the Deployment\AppDBootstrapper.zip file. The bootstrap policy should only execute when the condition of custom property APMEnable is set to Yes
- Node.js Application requirements
  - Your Node.js application needs to be deployed on Kubernetes
  - Your Node.js application needs to have a YAML file as the POD specification
  - Make sure you insert the snippet in Appendix A inside your node.js
  - When you deploy a new Node.js application in Apprenda, make sure to fill out the custom properties defined above with the right set of details for the AppDynamics environment
- Visit the AppDynamics portal to view performance metrics for your application


<h2>Apprendix A</h2>

> Insert this code inside your Node.js application to be monitored by AppDynamics. Apprenda and the bootstrap policy defined above will populate those environment variables, feeding the proper values to your application. The AppDynamics module inside your Node.js application will take this data and push the APM data to the appropriate AppDynamics server.

```
var appDobj = {
	 controllerHostName: process.env['APPD_URL'],
	 controllerPort: 443, 
	 controllerSslEnabled: true,
	 accountName: process.env['APPD_ACCOUNT'],
	 accountAccessKey: process.env['APPD_KEY'],
	 applicationName: process.env['APPD_APP_NAME'],
	 tierName: process.env['APPD_APP_TIER'],
	 nodeName: 'process'
}
require("appdynamics").profile(appDobj);
```
