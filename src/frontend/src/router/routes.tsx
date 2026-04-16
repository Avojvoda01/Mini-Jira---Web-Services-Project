import { createBrowserRouter, Navigate, useRouteError } from 'react-router-dom';
import { AppLayout } from '../components/layout/AppLayout';
import { ErrorState } from '../components/ui/ErrorState';
import { BacklogPage } from '../pages/BacklogPage';
import { BoardPage } from '../pages/BoardPage';
import { DashboardPage } from '../pages/DashboardPage';
import { NotFoundPage } from '../pages/NotFoundPage';
import { SettingsPage } from '../pages/SettingsPage';
import { TicketDetailsPage } from '../pages/TicketDetailsPage';

function RouteErrorElement() {
	const error = useRouteError();

	return (
		<div className="mx-auto flex min-h-[40vh] w-full max-w-2xl items-center justify-center px-4 py-8">
			<ErrorState
				title="Route Error"
				description={error instanceof Error ? error.message : 'Failed to render route.'}
			/>
		</div>
	);
}

export const router = createBrowserRouter([
	{
		path: '/',
		element: <AppLayout />,
		errorElement: <RouteErrorElement />,
		children: [
			{
				index: true,
				element: <Navigate to="/dashboard" replace />,
			},
			{
				path: 'dashboard',
				element: <DashboardPage />,
			},
			{
				path: 'board',
				element: <BoardPage />,
			},
			{
				path: 'backlog',
				element: <BacklogPage />,
			},
			{
				path: 'ticket/:ticketId',
				element: <TicketDetailsPage />,
			},
			{
				path: 'settings',
				element: <SettingsPage />,
			},
			{
				path: '*',
				element: <NotFoundPage />,
			},
		],
	},
]);
