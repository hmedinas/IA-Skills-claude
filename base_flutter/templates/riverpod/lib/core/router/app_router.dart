import 'package:__PackageName__/core/config/config_reader.dart';
import 'package:__PackageName__/core/constants/app_contans.dart';
import 'package:__PackageName__/features/auth/presentation/screens/login_screen.dart';
import 'package:__PackageName__/features/error_page/presentation/screens/error_screen.dart';
import 'package:__PackageName__/features/error_page/presentation/screens/not_found_screen.dart';
import 'package:__PackageName__/features/home/presentation/screens/home_bento_box_screen.dart';
import 'package:__PackageName__/features/home/presentation/screens/home_categori_screen.dart';
import 'package:__PackageName__/features/home/presentation/screens/home_dashboard_screen.dart';
import 'package:__PackageName__/features/home/presentation/screens/home_grid_screen.dart';
import 'package:__PackageName__/features/home/presentation/screens/home_radial_screen.dart';
import 'package:__PackageName__/features/home/presentation/screens/selection_business_grid_screen.dart';
import 'package:__PackageName__/features/home/presentation/screens/selection_bussiness_screen.dart';
import 'package:__PackageName__/features/initialization/presentation/screens/splash_screen.dart';
import 'package:flutter/material.dart';

class AppRouter {
    AppRouter._();

    // El Splash siempre es el punto de entrada (/)
    static String initialRoute(String environment) => AppRoutesScreen.splash;
   
    static Route<dynamic> onGenerateRoute(RouteSettings settings, String environment) 
    {
        WidgetBuilder? builder;
        switch (settings.name) {
            case '/':
            case AppRoutesScreen.splash:
                builder = (_) => SplashScreen(env: environment);
                break;
            case AppRoutesScreen.login:
                builder = (_) => const LoginScreen();
                break;
            case AppRoutesScreen.selectBusiness:
                builder = (_) => const SelectionBusinessScreen();
                break;
            case AppRoutesScreen.selectBusinessGrid:
                builder = (_) => const SelectionBusinessGridScreen();
                break;
            case AppRoutesScreen.homeBentoBox:
                builder = (_) => const HomeBentoBoxScreen();
                break;
            case AppRoutesScreen.homeCategori:
                builder = (_) => const HomeCategoriScreen();
                break;
            case AppRoutesScreen.homeDashboard:
                builder = (_) => const HomeDashboardScreen();
                break;
            case AppRoutesScreen.homeGrid:
                builder = (_) => const HomeGridScreen();
                break;
            case AppRoutesScreen.homeRadial:
                builder = (_) => const HomeRadialScreen();
                break;

            case AppRoutesScreen.error:
                final message = settings.arguments as String? ?? 'Error inesperado del servidor.';
                builder = (_) => ErrorScreen(message: message);
                break;
            case AppRoutesScreen.devDashboard:
               if (environment == 'dev' || environment == 'staging') 
                {
                    builder = (_) => const NotFoundScreen();
                } 
                else {
                    builder = (_) => const NotFoundScreen();
                }
                break;
            default:
                builder = (_) => const NotFoundScreen();
        }

        return MaterialPageRoute(
            builder: builder,
            settings: settings // importante para pasar arguments si hay
        );
    }
    

 /* es otra opcion pero hay el error cuando envias el enviromem por eso cambioamos a case */
/*
    static Route<dynamic> onGenerateRoute(RouteSettings settings) 
    {
        // Rutas de Auth
        final String _env = ConfigReader.currentEnv;

        WidgetBuilder? builder;

        final appRoutes = {
        '/': (_) => const SplashScreen(env: _env),
        'login': (_) => const LoginScreen(),
        'error': (context) {
            final message = settings.arguments as String? ?? 'Error inesperado del servidor.';
            return ErrorScreen(message: message);
        },
        };

        // Rutas de Dev (solo dev/staging)
        final devRoutes = {
        'devDashboard': (_) => const NotFoundScreen(),
        };

        if (appRoutes.containsKey(settings.name)) 
        {
            builder = appRoutes[settings.name];
        } 
        else
        {
            if ((_env == 'dev' || _env == 'staging') && devRoutes.containsKey(settings.name))
            {
                builder = devRoutes[settings.name];
            } 
        } 

        return MaterialPageRoute(
            builder: builder ?? (_) => const NotFoundScreen(),
            settings: settings // importante para pasar arguments si hay 
        );
    }
    */
}