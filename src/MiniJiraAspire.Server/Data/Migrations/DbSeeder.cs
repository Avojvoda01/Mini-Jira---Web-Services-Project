using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniJiraAspire.Server.Migrations;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Data.Migrations;

public class DbSeeder
{
    public static async Task MigrateAndSeedAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        if (db.Database.IsRelational())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();

        // ── 1. USERS ──────────────────────────────────────────────────────────
        // Password for all seeded accounts: Demo1234!
        var userDefs = new[]
        {
            // Admins
            ("quirin.brecheisen@web.de",          "Quirin Brecheisen",    UserRole.Admin),
            ("admin@mini-jira.local",              "System Administrator", UserRole.Admin),
            // Project owners (one per project below)
            ("sarah.mitchell@mini-jira.local",     "Sarah Mitchell",       UserRole.User),
            ("marcus.chen@mini-jira.local",        "Marcus Chen",          UserRole.User),
            ("elena.rossi@mini-jira.local",        "Elena Rossi",          UserRole.User),
            // Demo non-admin account for workflow demos
            ("lena.wagner@mini-jira.local",        "Lena Wagner",          UserRole.User),
            // Team members
            ("james.harrington@mini-jira.local",   "James Harrington",     UserRole.User),
            ("sophie.berger@mini-jira.local",      "Sophie Berger",        UserRole.User),
            ("noah.adebayo@mini-jira.local",       "Noah Adebayo",         UserRole.User),
            ("isabelle.fontaine@mini-jira.local",  "Isabelle Fontaine",    UserRole.User),
            ("lucas.patel@mini-jira.local",        "Lucas Patel",          UserRole.User),
            ("mia.kowalski@mini-jira.local",       "Mia Kowalski",         UserRole.User),
            ("ethan.brooks@mini-jira.local",       "Ethan Brooks",         UserRole.User),
            ("chloe.nakamura@mini-jira.local",     "Chloe Nakamura",       UserRole.User),
            ("oliver.mueller@mini-jira.local",     "Oliver Müller",        UserRole.User),
            ("amara.diallo@mini-jira.local",       "Amara Diallo",         UserRole.User),
            ("thomas.eriksson@mini-jira.local",    "Thomas Eriksson",      UserRole.User),
            ("priya.sharma@mini-jira.local",       "Priya Sharma",         UserRole.User),
            ("felix.zimmermann@mini-jira.local",   "Felix Zimmermann",     UserRole.User),
            ("hannah.obrien@mini-jira.local",      "Hannah O'Brien",       UserRole.User),
            ("diego.morales@mini-jira.local",      "Diego Morales",        UserRole.User),
        };

        foreach (var (email, displayName, role) in userDefs)
        {
            var existing = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existing is null)
            {
                var u = new User { Email = email, DisplayName = displayName, Role = role, PasswordHash = "" };
                u.PasswordHash = hasher.HashPassword(u, "Demo1234!");
                db.Users.Add(u);
            }
            else if (string.IsNullOrEmpty(existing.PasswordHash))
            {
                existing.PasswordHash = hasher.HashPassword(existing, "Demo1234!");
            }
        }

        await db.SaveChangesAsync();

        var quirin  = await db.Users.FirstOrDefaultAsync(u => u.Email == "quirin.brecheisen@web.de");
        var sarah   = await db.Users.FirstOrDefaultAsync(u => u.Email == "sarah.mitchell@mini-jira.local");
        var marcus  = await db.Users.FirstOrDefaultAsync(u => u.Email == "marcus.chen@mini-jira.local");
        var elena   = await db.Users.FirstOrDefaultAsync(u => u.Email == "elena.rossi@mini-jira.local");
        var lena    = await db.Users.FirstOrDefaultAsync(u => u.Email == "lena.wagner@mini-jira.local");
        var james   = await db.Users.FirstOrDefaultAsync(u => u.Email == "james.harrington@mini-jira.local");
        var sophie  = await db.Users.FirstOrDefaultAsync(u => u.Email == "sophie.berger@mini-jira.local");
        var noah    = await db.Users.FirstOrDefaultAsync(u => u.Email == "noah.adebayo@mini-jira.local");
        var isabelle= await db.Users.FirstOrDefaultAsync(u => u.Email == "isabelle.fontaine@mini-jira.local");
        var lucas   = await db.Users.FirstOrDefaultAsync(u => u.Email == "lucas.patel@mini-jira.local");
        var mia     = await db.Users.FirstOrDefaultAsync(u => u.Email == "mia.kowalski@mini-jira.local");
        var ethan   = await db.Users.FirstOrDefaultAsync(u => u.Email == "ethan.brooks@mini-jira.local");
        var chloe   = await db.Users.FirstOrDefaultAsync(u => u.Email == "chloe.nakamura@mini-jira.local");
        var oliver  = await db.Users.FirstOrDefaultAsync(u => u.Email == "oliver.mueller@mini-jira.local");
        var amara   = await db.Users.FirstOrDefaultAsync(u => u.Email == "amara.diallo@mini-jira.local");
        var thomas  = await db.Users.FirstOrDefaultAsync(u => u.Email == "thomas.eriksson@mini-jira.local");
        var priya   = await db.Users.FirstOrDefaultAsync(u => u.Email == "priya.sharma@mini-jira.local");
        var felix   = await db.Users.FirstOrDefaultAsync(u => u.Email == "felix.zimmermann@mini-jira.local");
        var hannah  = await db.Users.FirstOrDefaultAsync(u => u.Email == "hannah.obrien@mini-jira.local");
        var diego   = await db.Users.FirstOrDefaultAsync(u => u.Email == "diego.morales@mini-jira.local");

        // ── 2. PROJECTS ───────────────────────────────────────────────────────
        // P1 – Nexus Platform  (PO: Quirin)
        // P2 – Horizon CRM     (PO: Sarah)
        // P3 – Voyager Mobile  (PO: Marcus)
        // P4 – Aurora Analytics(PO: Elena)

        var p1 = await db.Projects.FirstOrDefaultAsync(p => p.Name == "Nexus Platform");
        if (p1 is null)
        {
            p1 = new Project
            {
                Name = "Nexus Platform",
                Description = "Enterprise SaaS platform for B2B workflow automation and team collaboration.",
                CreatedById = quirin?.Id,
            };
            db.Projects.Add(p1);
        }

        var p2 = await db.Projects.FirstOrDefaultAsync(p => p.Name == "Horizon CRM");
        if (p2 is null)
        {
            p2 = new Project
            {
                Name = "Horizon CRM",
                Description = "Customer relationship management platform built for mid-market sales teams.",
                CreatedById = sarah?.Id,
            };
            db.Projects.Add(p2);
        }

        var p3 = await db.Projects.FirstOrDefaultAsync(p => p.Name == "Voyager Mobile");
        if (p3 is null)
        {
            p3 = new Project
            {
                Name = "Voyager Mobile",
                Description = "Cross-platform mobile app for field teams, remote workers, and on-the-go task management.",
                CreatedById = lena?.Id,
            };
            db.Projects.Add(p3);
        }

        var p4 = await db.Projects.FirstOrDefaultAsync(p => p.Name == "Aurora Analytics");
        if (p4 is null)
        {
            p4 = new Project
            {
                Name = "Aurora Analytics",
                Description = "Real-time analytics and business intelligence platform powered by streaming data pipelines.",
                CreatedById = elena?.Id,
            };
            db.Projects.Add(p4);
        }

        await db.SaveChangesAsync();

        // ── 3. PROJECT MEMBERSHIPS ────────────────────────────────────────────
        // P1: Lena, James, Sophie, Noah, Isabelle, Lucas, Mia, Ethan, Chloe
        // P2: Quirin, Lena, Oliver, Amara, Thomas, Priya, Felix, Hannah, Diego
        // P3: Quirin, James, Sophie, Noah, Oliver, Amara, Felix, Hannah, Diego
        // P4: Quirin, Isabelle, Lucas, Mia, Ethan, Chloe, Thomas, Priya, Diego

        var memberships = new (Project proj, User? user)[]
        {
            (p1, lena), (p1, james), (p1, sophie), (p1, noah), (p1, isabelle),
            (p1, lucas), (p1, mia), (p1, ethan), (p1, chloe),

            (p2, quirin), (p2, lena), (p2, oliver), (p2, amara), (p2, thomas),
            (p2, priya), (p2, felix), (p2, hannah), (p2, diego),

            (p3, quirin), (p3, marcus), (p3, james), (p3, sophie), (p3, noah),
            (p3, oliver), (p3, amara), (p3, felix), (p3, hannah),

            (p4, quirin), (p4, isabelle), (p4, lucas), (p4, mia), (p4, ethan),
            (p4, chloe), (p4, thomas), (p4, priya), (p4, diego),
        };

        foreach (var (proj, user) in memberships)
        {
            if (user is null) continue;
            var exists = await db.ProjectMembers.AnyAsync(pm => pm.ProjectId == proj.Id && pm.UserId == user.Id);
            if (!exists)
                db.ProjectMembers.Add(new ProjectMember { ProjectId = proj.Id, UserId = user.Id });
        }

        await db.SaveChangesAsync();

        // ── 4. EPICS (5 per project) ──────────────────────────────────────────

        // --- P1: Nexus Platform ---
        if (!await db.Epics.AnyAsync(e => e.ProjectId == p1.Id))
        {
            db.Epics.AddRange(
                new Epic { Name = "Auth & Security",        Description = "Authentication, authorisation and audit infrastructure.", ProjectId = p1.Id, CreatedById = quirin?.Id },
                new Epic { Name = "Billing & Subscriptions",Description = "Stripe integration, invoice management and plan upgrades.", ProjectId = p1.Id, CreatedById = sophie?.Id },
                new Epic { Name = "Multi-tenant Core",      Description = "Tenant isolation, onboarding wizard and GDPR data export.", ProjectId = p1.Id, CreatedById = quirin?.Id },
                new Epic { Name = "Developer API & SDK",    Description = "Public REST API v2, TypeScript SDK and webhook delivery.", ProjectId = p1.Id, CreatedById = noah?.Id },
                new Epic { Name = "Admin Dashboard",        Description = "Global user management, health monitoring and audit trail.", ProjectId = p1.Id, CreatedById = quirin?.Id }
            );
        }

        // --- P2: Horizon CRM ---
        if (!await db.Epics.AnyAsync(e => e.ProjectId == p2.Id))
        {
            db.Epics.AddRange(
                new Epic { Name = "Contact Management",  Description = "Import, tagging, deduplication and timeline views for contacts.", ProjectId = p2.Id, CreatedById = sarah?.Id },
                new Epic { Name = "Sales Pipeline",      Description = "Kanban pipeline, deal stages, forecasting and follow-up reminders.", ProjectId = p2.Id, CreatedById = felix?.Id },
                new Epic { Name = "Email Campaigns",     Description = "Template builder, segmentation, A/B testing and compliance.", ProjectId = p2.Id, CreatedById = thomas?.Id },
                new Epic { Name = "Reporting",           Description = "Custom report builder, scheduled delivery and live dashboards.", ProjectId = p2.Id, CreatedById = quirin?.Id },
                new Epic { Name = "Mobile CRM",          Description = "React Native app, offline sync and business card OCR scanner.", ProjectId = p2.Id, CreatedById = felix?.Id }
            );
        }

        // --- P3: Voyager Mobile ---
        if (!await db.Epics.AnyAsync(e => e.ProjectId == p3.Id))
        {
            db.Epics.AddRange(
                new Epic { Name = "Onboarding & Auth",    Description = "First-run experience, biometric login and social sign-in.", ProjectId = p3.Id, CreatedById = lena?.Id },
                new Epic { Name = "Navigation & UX",      Description = "Tab navigation, deep linking, gestures and dark mode.", ProjectId = p3.Id, CreatedById = lena?.Id },
                new Epic { Name = "Offline Sync",         Description = "Local SQLite cache, conflict resolution and background sync.", ProjectId = p3.Id, CreatedById = quirin?.Id },
                new Epic { Name = "Push Notifications",   Description = "FCM integration, per-user preferences and rich notifications.", ProjectId = p3.Id, CreatedById = marcus?.Id },
                new Epic { Name = "Performance & Quality",Description = "Startup time, lazy loading, crash reporting and CI benchmarks.", ProjectId = p3.Id, CreatedById = felix?.Id }
            );
        }

        // --- P4: Aurora Analytics ---
        if (!await db.Epics.AnyAsync(e => e.ProjectId == p4.Id))
        {
            db.Epics.AddRange(
                new Epic { Name = "Data Ingestion",      Description = "Database connectors, Kafka streams and REST push endpoints.", ProjectId = p4.Id, CreatedById = elena?.Id },
                new Epic { Name = "Dashboard Builder",   Description = "Drag-and-drop canvas, 30+ chart types and shareable links.", ProjectId = p4.Id, CreatedById = chloe?.Id },
                new Epic { Name = "ML Integration",      Description = "Anomaly detection, forecasting and AutoML for business users.", ProjectId = p4.Id, CreatedById = priya?.Id },
                new Epic { Name = "Alerting System",     Description = "Rule-based alerts, multi-channel notifications and escalation.", ProjectId = p4.Id, CreatedById = ethan?.Id },
                new Epic { Name = "Export & APIs",       Description = "REST and GraphQL APIs, scheduled exports and OpenAPI docs.", ProjectId = p4.Id, CreatedById = chloe?.Id }
            );
        }

        await db.SaveChangesAsync();

        // ── 5. TASKS (20 per project) ─────────────────────────────────────────
        // EstimateMinutes: 30=30m 60=1h 120=2h 240=4h 360=6h 480=8h/1d 720=12h 960=2d 1440=3d

        // --- P1: Nexus Platform tasks ---
        if (!await db.TaskItems.AnyAsync(t => t.ProjectId == p1.Id))
        {
            var e1Auth  = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == p1.Id && e.Name == "Auth & Security");
            var e1Bill  = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == p1.Id && e.Name == "Billing & Subscriptions");
            var e1Tent  = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == p1.Id && e.Name == "Multi-tenant Core");
            var e1Api   = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == p1.Id && e.Name == "Developer API & SDK");
            var e1Admin = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == p1.Id && e.Name == "Admin Dashboard");

            db.TaskItems.AddRange(
                // Auth & Security
                new TaskItem { Title = "Implement OAuth 2.0 login with Google and GitHub", Description = "Set up OAuth 2.0 provider integrations so users can sign in with their existing Google or GitHub accounts. Include PKCE flow and session binding.", Status = "In Progress", Priority = "High", EstimateMinutes = 480, ProjectId = p1.Id, EpicId = e1Auth?.Id, CreatedById = quirin?.Id, AssigneeId = quirin?.Id },
                new TaskItem { Title = "Enable TOTP-based two-factor authentication", Description = "Add TOTP MFA using authenticator apps such as Google Authenticator. Generate QR-code setup flow and store encrypted secrets per user.", Status = "Open", Priority = "High", EstimateMinutes = 480, ProjectId = p1.Id, EpicId = e1Auth?.Id, CreatedById = quirin?.Id, AssigneeId = james?.Id },
                new TaskItem { Title = "Set up role-based access control middleware", Description = "Implement RBAC middleware that enforces permission checks at the API layer. Define Owner, Admin, Member and Viewer roles with configurable policy rules.", Status = "Done", Priority = "High", EstimateMinutes = 360, ProjectId = p1.Id, EpicId = e1Auth?.Id, CreatedById = lena?.Id, AssigneeId = lena?.Id },
                new TaskItem { Title = "Add security audit log for sensitive operations", Description = "Record a tamper-evident audit entry whenever a user changes credentials, alters roles or accesses restricted resources. Store in an append-only table.", Status = "Review", Priority = "Medium", EstimateMinutes = 240, ProjectId = p1.Id, EpicId = e1Auth?.Id, CreatedById = james?.Id, AssigneeId = noah?.Id },

                // Billing & Subscriptions
                new TaskItem { Title = "Integrate Stripe Checkout for subscription plans", Description = "Connect the Stripe SDK to support monthly and annual subscription plans. Handle checkout sessions, webhooks for payment confirmation and plan upgrades.", Status = "In Progress", Priority = "High", EstimateMinutes = 960, ProjectId = p1.Id, EpicId = e1Bill?.Id, CreatedById = sophie?.Id, AssigneeId = sophie?.Id },
                new TaskItem { Title = "Build subscription management UI for customers", Description = "Create a self-service portal where customers can view their current plan, upgrade or downgrade, and download invoices without contacting support.", Status = "Open", Priority = "Medium", EstimateMinutes = 480, ProjectId = p1.Id, EpicId = e1Bill?.Id, CreatedById = mia?.Id, AssigneeId = mia?.Id },
                new TaskItem { Title = "Implement invoice generation and PDF export", Description = "Auto-generate a PDF invoice for every successful charge including line items, tax breakdown and company branding. Store and serve via signed URLs.", Status = "Open", Priority = "Medium", EstimateMinutes = 360, ProjectId = p1.Id, EpicId = e1Bill?.Id, CreatedById = lucas?.Id, AssigneeId = lucas?.Id },
                new TaskItem { Title = "Handle failed payment retries with exponential back-off", Description = "Implement smart retry logic for failed charges with progressive delays of 1, 3 and 7 days. Notify the customer via email at each attempt and downgrade access after final failure.", Status = "Open", Priority = "Low", EstimateMinutes = 240, ProjectId = p1.Id, EpicId = e1Bill?.Id, CreatedById = isabelle?.Id, AssigneeId = isabelle?.Id },

                // Multi-tenant Core
                new TaskItem { Title = "Design tenant isolation strategy at the database layer", Description = "Evaluate and implement row-level security vs schema-per-tenant for PostgreSQL. Document the chosen approach and migration path from single to multi-tenant.", Status = "Done", Priority = "High", EstimateMinutes = 1440, ProjectId = p1.Id, EpicId = e1Tent?.Id, CreatedById = quirin?.Id, AssigneeId = quirin?.Id },
                new TaskItem { Title = "Build per-tenant configuration management API", Description = "Allow each tenant to configure their own feature flags, branding assets and notification preferences through a dedicated REST API scoped to their tenant ID.", Status = "In Progress", Priority = "High", EstimateMinutes = 480, ProjectId = p1.Id, EpicId = e1Tent?.Id, CreatedById = ethan?.Id, AssigneeId = ethan?.Id },
                new TaskItem { Title = "Create tenant onboarding wizard", Description = "Guide new tenants through workspace setup in four steps: invite team, configure SSO, set up billing and choose a default workflow template.", Status = "Open", Priority = "Medium", EstimateMinutes = 480, ProjectId = p1.Id, EpicId = e1Tent?.Id, CreatedById = chloe?.Id, AssigneeId = chloe?.Id },
                new TaskItem { Title = "GDPR-compliant data export and right to erasure", Description = "Allow tenant admins to request a full data export as JSON/CSV and trigger account deletion that anonymises all PII within 72 hours per GDPR Article 17.", Status = "Open", Priority = "Medium", EstimateMinutes = 360, ProjectId = p1.Id, EpicId = e1Tent?.Id, CreatedById = james?.Id, AssigneeId = james?.Id },

                // Developer API & SDK
                new TaskItem { Title = "Design and document REST API v2 schema", Description = "Define the v2 API contract with versioning headers, consistent error envelopes and cursor-based pagination. Review with the developer relations team before publishing.", Status = "Review", Priority = "High", EstimateMinutes = 960, ProjectId = p1.Id, EpicId = e1Api?.Id, CreatedById = noah?.Id, AssigneeId = noah?.Id },
                new TaskItem { Title = "Publish interactive Swagger and Scalar API docs", Description = "Auto-generate OpenAPI 3.1 spec from code annotations and host an interactive Scalar UI in production so developers can try endpoints without leaving the browser.", Status = "Done", Priority = "Medium", EstimateMinutes = 240, ProjectId = p1.Id, EpicId = e1Api?.Id, CreatedById = sophie?.Id, AssigneeId = sophie?.Id },
                new TaskItem { Title = "Build TypeScript SDK and publish to npm", Description = "Create a typed SDK wrapping the v2 API with automatic retry, token refresh and strongly typed response models. Ship on npm under the @nexus-platform scope.", Status = "In Progress", Priority = "Medium", EstimateMinutes = 1440, ProjectId = p1.Id, EpicId = e1Api?.Id, CreatedById = lucas?.Id, AssigneeId = lucas?.Id },
                new TaskItem { Title = "Implement webhook delivery with retry and HMAC signing", Description = "Deliver events to customer endpoints with HMAC-SHA256 request signing, an exponential retry policy and a dashboard showing per-endpoint delivery health.", Status = "Open", Priority = "High", EstimateMinutes = 480, ProjectId = p1.Id, EpicId = e1Api?.Id, CreatedById = mia?.Id, AssigneeId = mia?.Id },

                // Admin Dashboard
                new TaskItem { Title = "Global user management table with search and role filters", Description = "Build an admin-only page listing all users across tenants with live search, role filter pills, sortable columns and inline role assignment.", Status = "Done", Priority = "High", EstimateMinutes = 480, ProjectId = p1.Id, EpicId = e1Admin?.Id, CreatedById = quirin?.Id, AssigneeId = quirin?.Id },
                new TaskItem { Title = "System health monitoring panel with uptime graph", Description = "Show real-time service health indicators, a 30-day uptime graph and p95 latency for critical endpoints. Pull data from the OpenTelemetry pipeline.", Status = "In Progress", Priority = "Medium", EstimateMinutes = 360, ProjectId = p1.Id, EpicId = e1Admin?.Id, CreatedById = isabelle?.Id, AssigneeId = isabelle?.Id },
                new TaskItem { Title = "Bulk user import via CSV upload", Description = "Let admins upload a CSV of users with name, email and role columns. Validate, deduplicate and send invite emails in a background job with a progress indicator.", Status = "Open", Priority = "Low", EstimateMinutes = 240, ProjectId = p1.Id, EpicId = e1Admin?.Id, CreatedById = lena?.Id, AssigneeId = lena?.Id },
                new TaskItem { Title = "Activity feed for admin audit trail", Description = "Display a chronological feed of all admin actions — user role changes, project deletions, billing events — filterable by actor, action type and date range.", Status = "Review", Priority = "Medium", EstimateMinutes = 240, ProjectId = p1.Id, EpicId = e1Admin?.Id, CreatedById = chloe?.Id, AssigneeId = chloe?.Id }
            );
        }

        // --- P2: Horizon CRM tasks ---
        if (!await db.TaskItems.AnyAsync(t => t.ProjectId == p2.Id))
        {
            var e2Contact = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == p2.Id && e.Name == "Contact Management");
            var e2Sales   = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == p2.Id && e.Name == "Sales Pipeline");
            var e2Email   = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == p2.Id && e.Name == "Email Campaigns");
            var e2Report  = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == p2.Id && e.Name == "Reporting");
            var e2Mobile  = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == p2.Id && e.Name == "Mobile CRM");

            db.TaskItems.AddRange(
                // Contact Management
                new TaskItem { Title = "Implement contact import from CSV and vCard files", Description = "Parse CSV and .vcf uploads, map columns to contact fields with a preview step, flag duplicates and import in batches of 500 with a progress bar.", Status = "Done", Priority = "Medium", EstimateMinutes = 360, ProjectId = p2.Id, EpicId = e2Contact?.Id, CreatedById = sarah?.Id, AssigneeId = oliver?.Id },
                new TaskItem { Title = "Build contact tagging and segmentation system", Description = "Allow sales reps to apply free-form and predefined tags to contacts and create dynamic segments based on tag combinations for targeted outreach.", Status = "In Progress", Priority = "High", EstimateMinutes = 480, ProjectId = p2.Id, EpicId = e2Contact?.Id, CreatedById = sarah?.Id, AssigneeId = amara?.Id },
                new TaskItem { Title = "Add contact activity timeline view", Description = "Show a chronological timeline of all interactions with a contact: emails sent, calls logged, deals updated and notes added, with filter by activity type.", Status = "Open", Priority = "Medium", EstimateMinutes = 480, ProjectId = p2.Id, EpicId = e2Contact?.Id, CreatedById = oliver?.Id, AssigneeId = thomas?.Id },
                new TaskItem { Title = "Merge duplicate contact detection and resolution", Description = "Run a daily background job that scores contact pairs by name and email similarity, surfaces likely duplicates in a review queue and merges them on confirmation.", Status = "Open", Priority = "High", EstimateMinutes = 720, ProjectId = p2.Id, EpicId = e2Contact?.Id, CreatedById = priya?.Id, AssigneeId = priya?.Id },

                // Sales Pipeline
                new TaskItem { Title = "Drag-and-drop Kanban pipeline view", Description = "Build a smooth Kanban board for deal stages with drag-and-drop reordering, quick-edit cards, WIP limits per stage and a collapsed summary row showing total deal value.", Status = "Done", Priority = "High", EstimateMinutes = 960, ProjectId = p2.Id, EpicId = e2Sales?.Id, CreatedById = felix?.Id, AssigneeId = felix?.Id },
                new TaskItem { Title = "Automated deal stage progression rules", Description = "Let managers define trigger-based rules that auto-advance a deal stage when conditions are met, such as a signed proposal document being uploaded or a call being logged.", Status = "In Progress", Priority = "High", EstimateMinutes = 480, ProjectId = p2.Id, EpicId = e2Sales?.Id, CreatedById = hannah?.Id, AssigneeId = hannah?.Id },
                new TaskItem { Title = "Revenue forecast chart with close-probability weighting", Description = "Show a month-by-month forecast bar chart where deal values are weighted by close probability. Allow filter by owner, region and product line.", Status = "Review", Priority = "Medium", EstimateMinutes = 480, ProjectId = p2.Id, EpicId = e2Sales?.Id, CreatedById = diego?.Id, AssigneeId = diego?.Id },
                new TaskItem { Title = "Activity reminders and follow-up scheduling", Description = "Let reps schedule follow-up tasks linked to a deal. Send email and in-app reminders 24 hours and 1 hour before the due time with one-click snooze options.", Status = "Open", Priority = "Medium", EstimateMinutes = 240, ProjectId = p2.Id, EpicId = e2Sales?.Id, CreatedById = sarah?.Id, AssigneeId = quirin?.Id },

                // Email Campaigns
                new TaskItem { Title = "Visual drag-and-drop email template builder", Description = "Build a block-based email editor supporting text, image, button and divider blocks. Export to battle-tested HTML that renders correctly in Gmail, Outlook and Apple Mail.", Status = "In Progress", Priority = "High", EstimateMinutes = 1440, ProjectId = p2.Id, EpicId = e2Email?.Id, CreatedById = thomas?.Id, AssigneeId = thomas?.Id },
                new TaskItem { Title = "Audience segmentation for targeted campaigns", Description = "Allow marketers to define campaign audiences using contact fields, tags and behaviour triggers such as last-open date or deal stage. Preview segment size before sending.", Status = "Open", Priority = "High", EstimateMinutes = 480, ProjectId = p2.Id, EpicId = e2Email?.Id, CreatedById = amara?.Id, AssigneeId = amara?.Id },
                new TaskItem { Title = "A/B testing framework for email subject lines", Description = "Split a campaign send into two variants with different subject lines. After the test period, automatically send the winning variant to the remaining audience.", Status = "Open", Priority = "Medium", EstimateMinutes = 720, ProjectId = p2.Id, EpicId = e2Email?.Id, CreatedById = priya?.Id, AssigneeId = lena?.Id },
                new TaskItem { Title = "Unsubscribe handling and CAN-SPAM compliance", Description = "Add a one-click unsubscribe link to every campaign email, process list-unsubscribe headers and suppress unsubscribed contacts from all future sends within 10 business days.", Status = "Review", Priority = "High", EstimateMinutes = 360, ProjectId = p2.Id, EpicId = e2Email?.Id, CreatedById = sarah?.Id, AssigneeId = felix?.Id },

                // Reporting
                new TaskItem { Title = "Build customisable report builder with drag-and-drop columns", Description = "Let users compose reports from any CRM object fields by dragging columns, applying filters and choosing a grouping dimension. Save and share report configurations.", Status = "Open", Priority = "High", EstimateMinutes = 1440, ProjectId = p2.Id, EpicId = e2Report?.Id, CreatedById = quirin?.Id, AssigneeId = quirin?.Id },
                new TaskItem { Title = "Export reports to PDF, Excel and CSV", Description = "Add one-click export for any report in the builder. PDFs should include the company logo and chart snapshots; Excel exports should preserve formulas for calculated columns.", Status = "Done", Priority = "Medium", EstimateMinutes = 360, ProjectId = p2.Id, EpicId = e2Report?.Id, CreatedById = hannah?.Id, AssigneeId = hannah?.Id },
                new TaskItem { Title = "Real-time dashboard with live deal metrics", Description = "Show a manager dashboard with live widgets: total pipeline value, deals closed this month, average deal velocity and win-rate trend. Auto-refreshes every 30 seconds.", Status = "In Progress", Priority = "High", EstimateMinutes = 960, ProjectId = p2.Id, EpicId = e2Report?.Id, CreatedById = diego?.Id, AssigneeId = diego?.Id },
                new TaskItem { Title = "Scheduled report delivery via email", Description = "Allow managers to schedule any saved report to be emailed as a PDF attachment on a daily, weekly or monthly cadence with a configurable recipient list.", Status = "Open", Priority = "Low", EstimateMinutes = 240, ProjectId = p2.Id, EpicId = e2Report?.Id, CreatedById = sarah?.Id, AssigneeId = oliver?.Id },

                // Mobile CRM
                new TaskItem { Title = "React Native CRM app for iOS and Android", Description = "Bootstrap a React Native (Expo) project with shared navigation, authentication and API client. Set up CI builds for both platforms targeting iOS 16+ and Android 12+.", Status = "Open", Priority = "High", EstimateMinutes = 1440, ProjectId = p2.Id, EpicId = e2Mobile?.Id, CreatedById = felix?.Id, AssigneeId = felix?.Id },
                new TaskItem { Title = "Offline contact viewing with background sync", Description = "Cache a local copy of the user's contacts using SQLite. Allow read access while offline and queue any updates to sync automatically when connectivity is restored.", Status = "Open", Priority = "Medium", EstimateMinutes = 720, ProjectId = p2.Id, EpicId = e2Mobile?.Id, CreatedById = thomas?.Id, AssigneeId = thomas?.Id },
                new TaskItem { Title = "Mobile push notifications for deal stage updates", Description = "Trigger a push notification whenever a deal the rep owns moves stage or is commented on. Let users mute notifications per deal or during quiet hours.", Status = "In Progress", Priority = "Medium", EstimateMinutes = 480, ProjectId = p2.Id, EpicId = e2Mobile?.Id, CreatedById = amara?.Id, AssigneeId = amara?.Id },
                new TaskItem { Title = "Business card scanner with OCR to contact creation", Description = "Use the device camera and an OCR engine to scan printed business cards and pre-fill a new contact form with detected name, title, email and phone number.", Status = "Open", Priority = "Low", EstimateMinutes = 360, ProjectId = p2.Id, EpicId = e2Mobile?.Id, CreatedById = sarah?.Id, AssigneeId = priya?.Id }
            );
        }

        // --- P3: Voyager Mobile tasks ---
        if (!await db.TaskItems.AnyAsync(t => t.ProjectId == p3.Id))
        {
            var e3Onboard = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == p3.Id && e.Name == "Onboarding & Auth");
            var e3Nav     = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == p3.Id && e.Name == "Navigation & UX");
            var e3Offline = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == p3.Id && e.Name == "Offline Sync");
            var e3Push    = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == p3.Id && e.Name == "Push Notifications");
            var e3Perf    = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == p3.Id && e.Name == "Performance & Quality");

            db.TaskItems.AddRange(
                // Onboarding & Auth
                new TaskItem { Title = "Design onboarding carousel with five screens", Description = "Create an engaging five-screen onboarding flow introducing key app features. Include lottie animations and a skip option that lands the user directly on the sign-in screen.", Status = "Done", Priority = "Medium", EstimateMinutes = 240, ProjectId = p3.Id, EpicId = e3Onboard?.Id, CreatedById = lena?.Id, AssigneeId = sophie?.Id },
                new TaskItem { Title = "Implement biometric authentication (Face ID and fingerprint)", Description = "Use the device biometrics API to let returning users unlock the app instantly without entering a password. Fall back gracefully to PIN if biometrics are not enrolled.", Status = "In Progress", Priority = "High", EstimateMinutes = 480, ProjectId = p3.Id, EpicId = e3Onboard?.Id, CreatedById = lena?.Id, AssigneeId = james?.Id },
                new TaskItem { Title = "Add Sign in with Apple and Google Sign-In", Description = "Integrate Apple and Google OAuth flows following platform guidelines. Store the provider token and link the social identity to an existing account when the email matches.", Status = "Review", Priority = "High", EstimateMinutes = 480, ProjectId = p3.Id, EpicId = e3Onboard?.Id, CreatedById = noah?.Id, AssigneeId = noah?.Id },
                new TaskItem { Title = "Password reset with SMS OTP fallback", Description = "Send a six-digit OTP via email with an SMS fallback. Expire the token after 10 minutes and enforce a three-attempt limit before requiring a new code to be requested.", Status = "Open", Priority = "Medium", EstimateMinutes = 360, ProjectId = p3.Id, EpicId = e3Onboard?.Id, CreatedById = oliver?.Id, AssigneeId = oliver?.Id },

                // Navigation & UX
                new TaskItem { Title = "Implement bottom tab navigation with badge counters", Description = "Build a five-tab bottom navigation bar with Home, Tasks, Notifications, Search and Profile tabs. Show unread badge counts on Notifications and Tasks.", Status = "Done", Priority = "High", EstimateMinutes = 480, ProjectId = p3.Id, EpicId = e3Nav?.Id, CreatedById = lena?.Id, AssigneeId = marcus?.Id },
                new TaskItem { Title = "Deep linking support for push-driven navigation", Description = "Handle deep links such as voyager://tasks/123 to navigate directly to any task, project or notification detail screen from a push notification or shared URL.", Status = "In Progress", Priority = "Medium", EstimateMinutes = 480, ProjectId = p3.Id, EpicId = e3Nav?.Id, CreatedById = amara?.Id, AssigneeId = amara?.Id },
                new TaskItem { Title = "Gesture-based swipe actions on list items", Description = "Add left-swipe to reveal a quick-complete action and right-swipe to reveal a snooze action on task list rows. Animate the reveal with spring physics.", Status = "Open", Priority = "Low", EstimateMinutes = 360, ProjectId = p3.Id, EpicId = e3Nav?.Id, CreatedById = felix?.Id, AssigneeId = felix?.Id },
                new TaskItem { Title = "Dark mode following the system appearance setting", Description = "Fully support iOS and Android system dark mode. Audit all custom colours and swap them for semantic tokens so the theme switches without a restart.", Status = "Done", Priority = "Low", EstimateMinutes = 240, ProjectId = p3.Id, EpicId = e3Nav?.Id, CreatedById = hannah?.Id, AssigneeId = hannah?.Id },

                // Offline Sync
                new TaskItem { Title = "Local SQLite cache for offline task and project data", Description = "Use Expo SQLite to persist a snapshot of the user's tasks and projects. Define a migration-safe schema and populate the cache on first login and on each foreground resume.", Status = "In Progress", Priority = "High", EstimateMinutes = 1440, ProjectId = p3.Id, EpicId = e3Offline?.Id, CreatedById = quirin?.Id, AssigneeId = quirin?.Id },
                new TaskItem { Title = "Conflict resolution strategy for concurrent edits", Description = "Implement a last-write-wins strategy with server timestamp authority for most fields. For task status, present a conflict dialog when the server version differs from the local edit.", Status = "Open", Priority = "High", EstimateMinutes = 960, ProjectId = p3.Id, EpicId = e3Offline?.Id, CreatedById = james?.Id, AssigneeId = james?.Id },
                new TaskItem { Title = "Background sync triggered on network reconnect", Description = "Listen for network state changes and trigger a sync queue flush whenever connectivity is restored. Show a subtle banner informing the user that changes are being uploaded.", Status = "Open", Priority = "High", EstimateMinutes = 720, ProjectId = p3.Id, EpicId = e3Offline?.Id, CreatedById = noah?.Id, AssigneeId = noah?.Id },
                new TaskItem { Title = "Optimistic UI updates with rollback on server error", Description = "Apply local mutations immediately to give instant feedback, then reconcile with the server response. If the request fails, roll back the change and show an actionable error toast.", Status = "Review", Priority = "Medium", EstimateMinutes = 480, ProjectId = p3.Id, EpicId = e3Offline?.Id, CreatedById = diego?.Id, AssigneeId = diego?.Id },

                // Push Notifications
                new TaskItem { Title = "Integrate Firebase Cloud Messaging for push delivery", Description = "Register device tokens with FCM on login and unregister on logout. Handle foreground, background and quit-state notification payloads and route them to the correct screen.", Status = "Done", Priority = "High", EstimateMinutes = 480, ProjectId = p3.Id, EpicId = e3Push?.Id, CreatedById = marcus?.Id, AssigneeId = marcus?.Id },
                new TaskItem { Title = "Per-user notification preference settings screen", Description = "Let users toggle individual notification categories — task assignments, comments, due-date reminders, status changes — and configure a daily quiet-hours window.", Status = "Open", Priority = "Medium", EstimateMinutes = 360, ProjectId = p3.Id, EpicId = e3Push?.Id, CreatedById = sophie?.Id, AssigneeId = sophie?.Id },
                new TaskItem { Title = "Rich push notifications with inline action buttons", Description = "Send rich notifications that include the task title and the assigner's avatar. Add Complete and Snooze action buttons so users can respond without opening the app.", Status = "In Progress", Priority = "Medium", EstimateMinutes = 480, ProjectId = p3.Id, EpicId = e3Push?.Id, CreatedById = oliver?.Id, AssigneeId = oliver?.Id },
                new TaskItem { Title = "Notification delivery analytics dashboard (admin)", Description = "Show admins a dashboard with per-notification-type delivery rate, open rate and action rate over the last 30 days to help identify notification fatigue.", Status = "Open", Priority = "Low", EstimateMinutes = 240, ProjectId = p3.Id, EpicId = e3Push?.Id, CreatedById = amara?.Id, AssigneeId = amara?.Id },

                // Performance & Quality
                new TaskItem { Title = "Profile and reduce app cold start time to under 2 seconds", Description = "Measure current startup time with Flashlight and identify bottlenecks. Defer non-critical initialisations, enable Hermes and lazy-load heavy screens to hit the 2 s target.", Status = "In Progress", Priority = "High", EstimateMinutes = 960, ProjectId = p3.Id, EpicId = e3Perf?.Id, CreatedById = felix?.Id, AssigneeId = felix?.Id },
                new TaskItem { Title = "Lazy-load images with a blurred progressive placeholder", Description = "Replace all direct Image components with a lazy-loading wrapper that shows a blurred low-res placeholder first, then fades in the full-resolution image on load.", Status = "Done", Priority = "Medium", EstimateMinutes = 240, ProjectId = p3.Id, EpicId = e3Perf?.Id, CreatedById = hannah?.Id, AssigneeId = hannah?.Id },
                new TaskItem { Title = "Integrate Crashlytics for real-time crash reporting", Description = "Add Firebase Crashlytics to capture native and JS crashes with full stack traces, device context and user breadcrumb trails. Set up Slack alerts for new crash types.", Status = "Done", Priority = "Medium", EstimateMinutes = 360, ProjectId = p3.Id, EpicId = e3Perf?.Id, CreatedById = marcus?.Id, AssigneeId = marcus?.Id },
                new TaskItem { Title = "Automated performance regression tests in CI pipeline", Description = "Add a Detox E2E test suite measuring render times for the task list and dashboard screens. Fail the CI build if p95 frame render time exceeds 16 ms (60 fps threshold).", Status = "Open", Priority = "Medium", EstimateMinutes = 480, ProjectId = p3.Id, EpicId = e3Perf?.Id, CreatedById = quirin?.Id, AssigneeId = diego?.Id }
            );
        }

        // --- P4: Aurora Analytics tasks ---
        if (!await db.TaskItems.AnyAsync(t => t.ProjectId == p4.Id))
        {
            var e4Ingest  = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == p4.Id && e.Name == "Data Ingestion");
            var e4Dash    = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == p4.Id && e.Name == "Dashboard Builder");
            var e4ML      = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == p4.Id && e.Name == "ML Integration");
            var e4Alert   = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == p4.Id && e.Name == "Alerting System");
            var e4Export  = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == p4.Id && e.Name == "Export & APIs");

            db.TaskItems.AddRange(
                // Data Ingestion
                new TaskItem { Title = "Build connectors for PostgreSQL and MySQL data sources", Description = "Implement JDBC-style connectors for PostgreSQL and MySQL that snapshot tables on first sync and use CDC (logical replication) for incremental updates.", Status = "Done", Priority = "High", EstimateMinutes = 960, ProjectId = p4.Id, EpicId = e4Ingest?.Id, CreatedById = elena?.Id, AssigneeId = ethan?.Id },
                new TaskItem { Title = "Kafka stream ingestion with Confluent Schema Registry", Description = "Consume Avro-serialised events from Kafka topics, validate schemas against the registry, and write parsed records to the internal time-series store in real time.", Status = "In Progress", Priority = "High", EstimateMinutes = 1440, ProjectId = p4.Id, EpicId = e4Ingest?.Id, CreatedById = quirin?.Id, AssigneeId = quirin?.Id },
                new TaskItem { Title = "REST push endpoint with API key authentication", Description = "Expose a POST /ingest endpoint that accepts a JSON array of metric events. Authenticate via API key, rate-limit to 10 k events per minute and return a batch receipt ID.", Status = "Review", Priority = "Medium", EstimateMinutes = 480, ProjectId = p4.Id, EpicId = e4Ingest?.Id, CreatedById = lucas?.Id, AssigneeId = lucas?.Id },
                new TaskItem { Title = "Data validation pipeline with dead-letter queue", Description = "Add a validation stage that checks required fields, value ranges and schema conformance. Route invalid records to a dead-letter queue with a rejection reason for manual review.", Status = "Open", Priority = "High", EstimateMinutes = 720, ProjectId = p4.Id, EpicId = e4Ingest?.Id, CreatedById = isabelle?.Id, AssigneeId = isabelle?.Id },

                // Dashboard Builder
                new TaskItem { Title = "Drag-and-drop widget canvas with responsive grid snapping", Description = "Build a canvas where users can drag widgets from a sidebar, snap them to a 12-column grid and resize them. Persist layout as JSON and restore it on page load.", Status = "In Progress", Priority = "High", EstimateMinutes = 1440, ProjectId = p4.Id, EpicId = e4Dash?.Id, CreatedById = chloe?.Id, AssigneeId = chloe?.Id },
                new TaskItem { Title = "Implement 30+ chart types including heatmaps and Sankey diagrams", Description = "Integrate Apache ECharts to provide line, bar, pie, scatter, heatmap, treemap and Sankey chart types. Each chart type should support theming and animation controls.", Status = "In Progress", Priority = "High", EstimateMinutes = 960, ProjectId = p4.Id, EpicId = e4Dash?.Id, CreatedById = mia?.Id, AssigneeId = mia?.Id },
                new TaskItem { Title = "Dashboard template library with one-click apply", Description = "Curate 10 starter templates (E-commerce, SaaS Metrics, DevOps, etc.) that users can preview in a gallery and apply to a new dashboard with a single click.", Status = "Open", Priority = "Medium", EstimateMinutes = 480, ProjectId = p4.Id, EpicId = e4Dash?.Id, CreatedById = elena?.Id, AssigneeId = elena?.Id },
                new TaskItem { Title = "Share dashboard via public link with optional expiry", Description = "Generate a signed public URL for any dashboard that can be opened without authentication. Allow the owner to set an expiry date and revoke the link at any time.", Status = "Open", Priority = "Medium", EstimateMinutes = 360, ProjectId = p4.Id, EpicId = e4Dash?.Id, CreatedById = thomas?.Id, AssigneeId = thomas?.Id },

                // ML Integration
                new TaskItem { Title = "Anomaly detection plugin using Isolation Forest", Description = "Wrap scikit-learn's IsolationForest in a micro-service that scores incoming time-series windows. Surface detected anomalies as annotations on the relevant line chart.", Status = "Open", Priority = "High", EstimateMinutes = 1440, ProjectId = p4.Id, EpicId = e4ML?.Id, CreatedById = priya?.Id, AssigneeId = priya?.Id },
                new TaskItem { Title = "Time-series forecasting with Meta Prophet integration", Description = "Allow users to apply a Prophet model to any metric time series with a configurable forecast horizon. Display confidence intervals and trend decomposition alongside the raw data.", Status = "Open", Priority = "High", EstimateMinutes = 1440, ProjectId = p4.Id, EpicId = e4ML?.Id, CreatedById = diego?.Id, AssigneeId = diego?.Id },
                new TaskItem { Title = "AutoML model training UI for business analysts", Description = "Build a no-code UI where analysts pick a target metric and training features, then kick off an AutoML training run. Show model accuracy metrics and a feature importance chart when done.", Status = "Open", Priority = "Medium", EstimateMinutes = 960, ProjectId = p4.Id, EpicId = e4ML?.Id, CreatedById = elena?.Id, AssigneeId = elena?.Id },
                new TaskItem { Title = "Model versioning and A/B deployment support", Description = "Store each trained model version with metadata and evaluation scores. Let admins route a percentage of scoring requests to a challenger model to compare live performance.", Status = "Review", Priority = "High", EstimateMinutes = 720, ProjectId = p4.Id, EpicId = e4ML?.Id, CreatedById = quirin?.Id, AssigneeId = quirin?.Id },

                // Alerting System
                new TaskItem { Title = "Rule-based alert engine with threshold conditions", Description = "Let users define alerts using conditions such as 'metric > threshold for N consecutive windows'. Evaluate rules against the streaming pipeline and fire when conditions are met.", Status = "Done", Priority = "High", EstimateMinutes = 480, ProjectId = p4.Id, EpicId = e4Alert?.Id, CreatedById = ethan?.Id, AssigneeId = ethan?.Id },
                new TaskItem { Title = "Multi-channel notifications: email, Slack and PagerDuty", Description = "Route alert events to email, Slack incoming webhooks and PagerDuty incidents based on severity and the user's channel preferences. Support templated messages.", Status = "In Progress", Priority = "High", EstimateMinutes = 480, ProjectId = p4.Id, EpicId = e4Alert?.Id, CreatedById = isabelle?.Id, AssigneeId = isabelle?.Id },
                new TaskItem { Title = "Alert snooze and acknowledge workflow", Description = "Add snooze (15 min, 1 h, 4 h) and acknowledge actions to each alert. Acknowledged alerts suppress re-notification until the condition clears and re-triggers.", Status = "Open", Priority = "Medium", EstimateMinutes = 240, ProjectId = p4.Id, EpicId = e4Alert?.Id, CreatedById = lucas?.Id, AssigneeId = lucas?.Id },
                new TaskItem { Title = "On-call escalation policy with rotation schedule", Description = "Define escalation tiers where an alert escalates to the next on-call engineer if not acknowledged within a configurable timeout. Integrate with a weekly rotation schedule.", Status = "Open", Priority = "Medium", EstimateMinutes = 720, ProjectId = p4.Id, EpicId = e4Alert?.Id, CreatedById = mia?.Id, AssigneeId = mia?.Id },

                // Export & APIs
                new TaskItem { Title = "REST API for querying metrics with time-range and aggregation filters", Description = "Expose GET /metrics with query parameters for metric name, time range, granularity and aggregation function (sum, avg, p95). Return paginated JSON with cursor navigation.", Status = "Done", Priority = "High", EstimateMinutes = 480, ProjectId = p4.Id, EpicId = e4Export?.Id, CreatedById = chloe?.Id, AssigneeId = chloe?.Id },
                new TaskItem { Title = "Scheduled CSV and Excel export delivered via email", Description = "Allow users to schedule any metric or dashboard export to run daily, weekly or monthly and deliver the file as an email attachment with a configurable recipient list.", Status = "In Progress", Priority = "Medium", EstimateMinutes = 360, ProjectId = p4.Id, EpicId = e4Export?.Id, CreatedById = thomas?.Id, AssigneeId = thomas?.Id },
                new TaskItem { Title = "GraphQL API for flexible data querying", Description = "Expose a GraphQL endpoint that mirrors the REST API capabilities. Generate the schema from the same metadata layer so new metric types are automatically queryable.", Status = "Open", Priority = "High", EstimateMinutes = 960, ProjectId = p4.Id, EpicId = e4Export?.Id, CreatedById = priya?.Id, AssigneeId = priya?.Id },
                new TaskItem { Title = "Auto-generate OpenAPI spec and Postman collection", Description = "Publish a versioned OpenAPI 3.1 spec from the live API and auto-generate a Postman collection that can be imported with one click. Update on every deploy.", Status = "Open", Priority = "Low", EstimateMinutes = 240, ProjectId = p4.Id, EpicId = e4Export?.Id, CreatedById = elena?.Id, AssigneeId = diego?.Id }
            );
        }

        await db.SaveChangesAsync();

        // ── 6. COMMENTS (1–2 per task) ────────────────────────────────────────
        // Organisational (scheduling, blockers, hand-offs) and technical notes,
        // authored by project members and keyed to tasks by their unique title.
        if (!await db.Comments.AnyAsync())
        {
            var tasksByTitle = await db.TaskItems.ToDictionaryAsync(t => t.Title);

            void AddComment(string taskTitle, User? author, string content)
            {
                if (author is null || !tasksByTitle.TryGetValue(taskTitle, out var task))
                    return;
                db.Comments.Add(new Comment { TaskId = task.Id.ToString(), UserId = author.Id, Content = content });
            }

            // --- P1: Nexus Platform ---
            AddComment("Implement OAuth 2.0 login with Google and GitHub", james, "PKCE flow is wired up for Google. Still need to register the GitHub OAuth app and get the client secret from infra.");
            AddComment("Implement OAuth 2.0 login with Google and GitHub", sophie, "Please whitelist the redirect URIs for the staging domain too, otherwise QA can't test this.");
            AddComment("Enable TOTP-based two-factor authentication", quirin, "Let's hold this until OAuth login is merged to avoid conflicts in the auth module.");
            AddComment("Set up role-based access control middleware", lena, "Merged and deployed. Owner/Admin/Member/Viewer policies are documented in the wiki.");
            AddComment("Add security audit log for sensitive operations", james, "Looks good overall, but please use an append-only table with no UPDATE/DELETE grants so it's genuinely tamper-evident.");
            AddComment("Integrate Stripe Checkout for subscription plans", sophie, "Checkout sessions work in test mode. Webhook signature verification is next.");
            AddComment("Integrate Stripe Checkout for subscription plans", mia, "Can we sync on the plan model before this is done? The subscription UI depends on the final field names.");
            AddComment("Build subscription management UI for customers", mia, "Blocked on the Stripe integration — I need the live plan and invoice data shape first.");
            AddComment("Implement invoice generation and PDF export", lucas, "Evaluating QuestPDF vs a headless-Chrome render. Leaning QuestPDF for the smaller footprint.");
            AddComment("Handle failed payment retries with exponential back-off", isabelle, "Low priority for now — let's revisit after the core billing flow ships.");
            AddComment("Design tenant isolation strategy at the database layer", quirin, "Decision: row-level security on a shared schema. Migration path is documented in docs/arc.md.");
            AddComment("Design tenant isolation strategy at the database layer", ethan, "This unblocks the per-tenant config API — picking it up now.");
            AddComment("Build per-tenant configuration management API", ethan, "Feature-flag and branding endpoints are done. Notification preferences still to do.");
            AddComment("Create tenant onboarding wizard", chloe, "Design mockups are ready. Waiting on the SSO config endpoint before I start the implementation.");
            AddComment("GDPR-compliant data export and right to erasure", james, "Need legal to confirm the 72-hour anonymisation window before we commit to it in the UI copy.");
            AddComment("Design and document REST API v2 schema", sophie, "Reviewed — please switch the error envelope to RFC 7807 ProblemDetails for consistency with the rest of the API.");
            AddComment("Publish interactive Swagger and Scalar API docs", sophie, "Live on /scalar in production. The OpenAPI spec is generated on build.");
            AddComment("Build TypeScript SDK and publish to npm", lucas, "Core client and auto-retry done. The token-refresh interceptor is flaky under concurrent requests — investigating.");
            AddComment("Implement webhook delivery with retry and HMAC signing", mia, "Moving this to next sprint — capacity is tight and the SDK work takes priority.");
            AddComment("Global user management table with search and role filters", quirin, "Shipped. Inline role assignment is admin-only and goes through the audit log.");
            AddComment("System health monitoring panel with uptime graph", isabelle, "OpenTelemetry data is flowing. Still tuning the p95 latency query — it's slow over large time ranges.");
            AddComment("Bulk user import via CSV upload", lena, "We should reuse the CSV parsing util from the Horizon CRM contact import rather than rolling our own.");
            AddComment("Activity feed for admin audit trail", chloe, "Filters work. Pagination needs to be cursor-based instead of offset — the feed grows quickly.");

            // --- P2: Horizon CRM ---
            AddComment("Implement contact import from CSV and vCard files", oliver, "Shipped with a column-mapping preview step. Handles batches of 500 with a progress bar.");
            AddComment("Build contact tagging and segmentation system", sarah, "Please make sure dynamic segments recompute when a contact's tags change, not just on the nightly job.");
            AddComment("Add contact activity timeline view", thomas, "Waiting on the tagging work to land so I can include tag changes in the timeline.");
            AddComment("Merge duplicate contact detection and resolution", priya, "Prototyping the similarity scoring — Levenshtein on name plus exact email match looks promising.");
            AddComment("Drag-and-drop Kanban pipeline view", felix, "Live. WIP limits per stage are configurable and the total-value summary row is in.");
            AddComment("Drag-and-drop Kanban pipeline view", hannah, "The sales team love it — already getting requests for swimlanes by owner.");
            AddComment("Automated deal stage progression rules", hannah, "Rule-builder UI is done. Hooking up the document-upload trigger next.");
            AddComment("Revenue forecast chart with close-probability weighting", diego, "Numbers match the spreadsheet the sales team uses today. Ready for a final look.");
            AddComment("Activity reminders and follow-up scheduling", sarah, "Can we move this up? Follow-up reminders are the top request from reps this quarter.");
            AddComment("Visual drag-and-drop email template builder", thomas, "Block editor works. Outlook rendering is the usual nightmare — testing with Litmus.");
            AddComment("Audience segmentation for targeted campaigns", amara, "This depends on the contact segmentation system — let's not duplicate the segment logic across both.");
            AddComment("A/B testing framework for email subject lines", priya, "We need to define how the winner is picked — open rate after 4 hours, or click rate? Needs product input before building.");
            AddComment("Unsubscribe handling and CAN-SPAM compliance", sarah, "This is a legal must-have before any campaign goes out. Prioritising the review.");
            AddComment("Build customisable report builder with drag-and-drop columns", quirin, "Big one. Let's split it into a read-only report viewer first, then add the drag-and-drop builder.");
            AddComment("Export reports to PDF, Excel and CSV", hannah, "All three formats shipped. Excel keeps the formulas for calculated columns.");
            AddComment("Real-time dashboard with live deal metrics", diego, "Widgets refresh every 30s over SignalR. Keeping an eye on DB load from the win-rate query.");
            AddComment("Scheduled report delivery via email", oliver, "Can reuse the scheduling infra from Aurora's scheduled export — already talking to that team.");
            AddComment("React Native CRM app for iOS and Android", felix, "Expo project scaffolded locally. Need Apple Developer and Play Console accounts from infra before CI builds.");
            AddComment("Offline contact viewing with background sync", thomas, "Blocked on the app scaffold landing first.");
            AddComment("Mobile push notifications for deal stage updates", amara, "FCM tokens are registering. The quiet-hours muting is the tricky part.");
            AddComment("Business card scanner with OCR to contact creation", priya, "Nice-to-have — moving to the backlog until the core mobile app is stable.");

            // --- P3: Voyager Mobile ---
            AddComment("Design onboarding carousel with five screens", sophie, "Shipped with Lottie animations and a skip-to-sign-in option.");
            AddComment("Implement biometric authentication (Face ID and fingerprint)", james, "Face ID works on iOS. The Android fingerprint fallback to PIN is wired up but needs more device testing.");
            AddComment("Implement biometric authentication (Face ID and fingerprint)", lena, "Don't forget the lockout after repeated biometric failures — security flagged it.");
            AddComment("Add Sign in with Apple and Google Sign-In", noah, "Both flows work. Apple requires the account-linking case when emails match — that's covered, please verify.");
            AddComment("Password reset with SMS OTP fallback", oliver, "Need a budget decision on the SMS provider (Twilio vs Vonage) before I wire up the fallback.");
            AddComment("Implement bottom tab navigation with badge counters", marcus, "Five tabs live. Unread badges on Notifications and Tasks update in real time.");
            AddComment("Deep linking support for push-driven navigation", amara, "The voyager:// scheme is registered. Cold-start deep links need the nav stack ready first — handling that now.");
            AddComment("Gesture-based swipe actions on list items", felix, "Low priority — parking this until the cold-start performance work is done.");
            AddComment("Dark mode following the system appearance setting", hannah, "All colours moved to semantic tokens. Switches without a restart.");
            AddComment("Local SQLite cache for offline task and project data", quirin, "Schema and first-login population done. Working on the migration-safe upgrade path.");
            AddComment("Conflict resolution strategy for concurrent edits", james, "Last-write-wins is fine for most fields, but status needs the conflict dialog. Depends on the cache landing first.");
            AddComment("Background sync triggered on network reconnect", noah, "Blocked on the offline cache and conflict resolution — sequencing this after both.");
            AddComment("Optimistic UI updates with rollback on server error", diego, "Rollback works and the error toast is actionable. Please review the reconciliation logic.");
            AddComment("Integrate Firebase Cloud Messaging for push delivery", marcus, "FCM is live. Foreground, background and quit-state payloads all route to the right screen.");
            AddComment("Per-user notification preference settings screen", sophie, "Waiting on the final list of notification categories from product before building the toggles.");
            AddComment("Rich push notifications with inline action buttons", oliver, "Complete/Snooze actions work on iOS. Android needs a custom notification layout.");
            AddComment("Notification delivery analytics dashboard (admin)", amara, "Low priority — moving to next sprint, it needs enough delivery data to accumulate first.");
            AddComment("Profile and reduce app cold start time to under 2 seconds", felix, "Hermes enabled, down to 2.6s. Deferring the analytics SDK init should get us under 2s.");
            AddComment("Lazy-load images with a blurred progressive placeholder", hannah, "All Image components swapped for the lazy wrapper — noticeable scroll improvement.");
            AddComment("Integrate Crashlytics for real-time crash reporting", marcus, "Live with Slack alerts for new crash types. Source maps upload on each release build.");
            AddComment("Automated performance regression tests in CI pipeline", quirin, "Let's gate this behind the cold-start work so we benchmark the optimised build, not the current one.");

            // --- P4: Aurora Analytics ---
            AddComment("Build connectors for PostgreSQL and MySQL data sources", ethan, "Both connectors live with CDC incremental sync. MySQL needs binlog enabled on the source.");
            AddComment("Kafka stream ingestion with Confluent Schema Registry", quirin, "Avro deserialisation works against the registry. Tuning consumer lag under high throughput.");
            AddComment("Kafka stream ingestion with Confluent Schema Registry", isabelle, "Once this is stable I'll point the validation pipeline at its output.");
            AddComment("REST push endpoint with API key authentication", lucas, "The 10k/min rate limit is in. Please review the batch-receipt ID format before I lock the contract.");
            AddComment("Data validation pipeline with dead-letter queue", isabelle, "DLQ schema is designed. Need to agree on the rejection-reason taxonomy with the ingestion team.");
            AddComment("Drag-and-drop widget canvas with responsive grid snapping", chloe, "12-column snapping works and layout persists as JSON. Resize handles are still glitchy on touch.");
            AddComment("Implement 30+ chart types including heatmaps and Sankey diagrams", mia, "ECharts integrated, 22 types done. Sankey and treemap theming left.");
            AddComment("Dashboard template library with one-click apply", elena, "Depends on the widget canvas being stable before I can author the starter templates.");
            AddComment("Share dashboard via public link with optional expiry", thomas, "Security review needed — public links must not leak the underlying raw-data queries.");
            AddComment("Anomaly detection plugin using Isolation Forest", priya, "Spinning up the scoring micro-service. Need a decision on a Python sidecar vs ONNX in-process.");
            AddComment("Time-series forecasting with Meta Prophet integration", diego, "Prophet is heavy. Proposing we run it async and cache forecasts rather than computing on every render.");
            AddComment("AutoML model training UI for business analysts", elena, "Parking this until anomaly detection and forecasting prove out the ML serving infra.");
            AddComment("Model versioning and A/B deployment support", quirin, "Challenger routing by percentage works. Please review the model-metadata schema.");
            AddComment("Rule-based alert engine with threshold conditions", ethan, "Live against the streaming pipeline. The 'N consecutive windows' condition is supported.");
            AddComment("Multi-channel notifications: email, Slack and PagerDuty", isabelle, "Email and Slack done. PagerDuty incident creation is next — waiting on an API key from ops.");
            AddComment("Alert snooze and acknowledge workflow", lucas, "Straightforward once the alert engine's state model is final — syncing with Ethan.");
            AddComment("On-call escalation policy with rotation schedule", mia, "Moving to next sprint. Needs the acknowledge workflow in place first to know when to escalate.");
            AddComment("REST API for querying metrics with time-range and aggregation filters", chloe, "Shipped with cursor pagination and sum/avg/p95 aggregations.");
            AddComment("Scheduled CSV and Excel export delivered via email", thomas, "Scheduling and CSV done. The Excel export times out on large datasets — chunking it.");
            AddComment("GraphQL API for flexible data querying", priya, "The schema will be generated from the same metadata layer as REST so the two don't drift.");
            AddComment("Auto-generate OpenAPI spec and Postman collection", diego, "Low priority. Will hook this into the deploy pipeline once the REST contract is frozen.");

            await db.SaveChangesAsync();
        }
    }
}
