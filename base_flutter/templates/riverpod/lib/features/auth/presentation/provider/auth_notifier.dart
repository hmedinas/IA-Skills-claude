import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:__PackageName__/features/auth/data/models/auth_state.dart';
import 'package:__PackageName__/features/auth/data/models/login_response_model.dart';
import 'package:__PackageName__/features/auth/data/sources/auth_api.dart';

// Estado del formulario de login. Usa autoDispose porque, a diferencia de la
// sesión (sessionProvider), este estado no debe persistir una vez se abandona
// la pantalla de login.
final authProvider = NotifierProvider.autoDispose<AuthNotifier, AuthState>(
  AuthNotifier.new,
);

class AuthNotifier extends Notifier<AuthState> {
  late final AuthApi _authService;

  @override
  AuthState build() {
    _authService = AuthApi();
    return AuthState();
  }

  void setUserName(String value) =>
      state = state.copyWith(userName: value, errorMessage: null);

  void setPassword(String value) =>
      state = state.copyWith(password: value, errorMessage: null);

  bool isValidForm() {
    return state.userName.trim().length >= 3 && state.password.trim().length >= 3;
  }

  Future<LoginResponseModel?> login() async {
    state = state.copyWith(isLoading: true, errorMessage: null);

    try {
      final response = await _authService.login(state.userName, state.password);
      state = state.copyWith(isLoading: false);
      return response;
    } catch (e) {
      state = state.copyWith(isLoading: false, errorMessage: "Error de red");
      return null;
    }
  }
}
