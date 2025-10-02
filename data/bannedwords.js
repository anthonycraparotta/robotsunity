const RAW_BANNED_WORDS = [
  'nigger', 'n i g g e r', 'n1gger', 'nigg3r', 'n1gg3r', 'n-i-g-g-e-r', 'nigg',
  'fag', 'f a g', 'f4g', 'f ag', 'fa g', 'f-a-g',
  'nazi', 'n a z i',
  'r@pe', 'rap3', 'rape', 'r4pe', 'r4p3', 'r a p e',
  'kill yourself', 'kill urself', 'kill yaself',
  'wh0r', 'whor3', 'whore', 'w-h-o-r-e', 'w h o r e', 'wh-ore', 'w-hor3', 'w_h_o_r_e', 'h03', 'ho3', 'hoe',
  'h0e', 'h o e', 'h_o_e', 'h.o.e', 'h.o_e', 'h_o.e', 'slut', 's-l-u-t', 's l u t', 's_l_u_t', 's.l.u.t',
  'slu t', 'stfu', 'tw4t', 'twat', 't w a t', 't w 4 t', 't-w-a-t', 't-w-4-t', 'twa t', 't.w.a.t', 't_w_a_t',
  'kike', 'kyke',
  'tranny', 'batty boy', 'sissy', 'fudgepacker',
  'chink', 'beaner', 'ching chong', 'wetback', 'wet-back', 'wet back', 'gook', 'goy', 'gypsy', 'honky', 'honkey', 'honkie', 'towel head',
  'pedo', 'groomer', 'rapist', 'rapeist', 'p3do', 'ped0', 'p3d0', 'fondle', 'molest', 'hooker', 'cunt', 'c-u-n-t', 'c u n t',
  'whitey'
];

const normalizeBannedText = (text) => {
  if (typeof text !== 'string') {
    return '';
  }

  return text
    .toLowerCase()
    .normalize('NFKD')
    .replace(/[^a-z0-9]/g, '');
};

const bannedWords = Array.from(
  new Set(RAW_BANNED_WORDS.map(normalizeBannedText).filter(Boolean))
);

export { bannedWords, normalizeBannedText };
export default { bannedWords, normalizeBannedText };
