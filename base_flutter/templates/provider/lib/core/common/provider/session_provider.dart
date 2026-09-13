
// creamos un provider Global de sesión (ChangeNotifier)
import 'package:__PackageName__/core/common/domain/entities/business_models.dart';
import 'package:__PackageName__/core/common/domain/entities/user_models.dart';
import 'package:__PackageName__/core/utils/console.dart';
import 'package:__PackageName__/features/auth/data/models/login_response_model.dart';
import 'package:flutter/foundation.dart';

// Recuerda que este provider se registra una sola vez en AppStateProvider,
// porque estos datos, como en pinia, tienen que persistir en memoria mientras
// dure la sesión.
class SessionProvider extends ChangeNotifier {

  LoginResponseModel? _session;

  LoginResponseModel? get session => _session;

  // Accion 1: Guardar Datos despues del login
  void updateSession(LoginResponseModel? userData) {
    _session = userData;
    notifyListeners();
  }

  // Accion 2: Seleccionar una empresa
  void setBusiness(BusinessModels empresa) {
    if (_session != null) {
      // "Copiamos" el estado actual pero cambiando solo la empresa seleccionada
      _session = _session!.copyWith(businessSelectModels: empresa);

      Console.log('🏢 Empresa seleccionada: ${empresa.empresa}', layer: 'SESSION');
      notifyListeners();
    }
  }

  // Cerrar sesión
  void logout() {
    _session = null;
    notifyListeners();
  }

  // Verificar si el usuario tiene un rol específico
  bool hasRole(String role) {
    final user = _session?.user;
    return user != null && user.roles.contains(role);
  }

  // Verificar si el usuario tiene algún rol de una lista
  bool hasAnyRole(List<String> roles) {
    final user = _session?.user;
    return user != null && roles.any(user.roles.contains);
  }

  // Verificar si el usuario tiene todos los roles de una lista
  bool hasAllRoles(List<String> roles) {
    final user = _session?.user;
    return user != null && roles.every(user.roles.contains);
  }

  // Verificar si hay sesión activa
  bool get isLogged => _session?.isAuthenticated ?? false;
  String? get token => _session?.accessToken;
  UserModels? get user => _session?.user;

  // Obtener datos del usuario logueado
  UserModels? getUserData() => _session?.user;

  // Obtener el token de acceso
  String? getAccessToken() => _session?.accessToken;

}
