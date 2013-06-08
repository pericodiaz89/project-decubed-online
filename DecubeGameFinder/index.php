<?php
$action = $_REQUEST['action'];
$index = new index();
if ($action == 'registrar') {
    $index->registrarServidor();
} else if ($action == 'buscar') {
    $index->buscarJuego();
}

class index {

    public function registrarServidor() {
        $ip = apc_fetch('direcciones');
        if (!($ip)) {
            $ip = array();
        }
        $encontrarIP = FALSE;

        for ($x = 0; $x < count($ip); $x++) {
            if ($_SERVER['REMOTE_ADDR'] == $ip[$x]) {
                $encontrarIP = TRUE;
                break;
            }
        }
        if ($encontrarIP == FALSE) {
            array_push($ip, $_SERVER['REMOTE_ADDR']);
            apc_store('direcciones', $ip);
        }
        echo "Success";
    }

    public function buscarJuego() {
        $ip = apc_fetch('direcciones');
        if (!($ip)) {
            echo "empty";
            die;
        }
        echo array_pop($ip);
        apc_store('direcciones', $ip);
    }

}
?>