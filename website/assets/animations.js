// Lightweight scroll-reveal: fades and slides elements in as they enter the
// viewport. Progressive enhancement only, everything is fully visible and
// usable without JavaScript or if IntersectionObserver isn't supported.
document.addEventListener('DOMContentLoaded', function () {
  if (!('IntersectionObserver' in window)) return;

  var targets = document.querySelectorAll(
    '.feature, .blog-card, .faq-item, .partners .feature, section .section-heading, section .section-sub'
  );

  targets.forEach(function (el) {
    el.classList.add('reveal');
  });

  var observer = new IntersectionObserver(
    function (entries) {
      entries.forEach(function (entry) {
        if (entry.isIntersecting) {
          entry.target.classList.add('is-visible');
          observer.unobserve(entry.target);
        }
      });
    },
    { threshold: 0.08, rootMargin: '0px 0px -30px 0px' }
  );

  targets.forEach(function (el) {
    observer.observe(el);
  });
});
